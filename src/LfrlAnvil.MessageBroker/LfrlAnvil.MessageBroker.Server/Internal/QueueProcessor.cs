// Copyright 2025 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct QueueProcessor
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private Task? _task;

    private QueueProcessor(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _task = null;
    }

    [Pure]
    internal static QueueProcessor Create()
    {
        return new QueueProcessor( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerQueue queue)
    {
        while ( true )
        {
            TaskStopReason stopReason;
            try
            {
                stopReason = await RunCore( queue ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                queue.Emit( MessageBrokerQueueEvent.Unexpected( queue, exc ) );
                stopReason = TaskStopReason.Error;
            }

            if ( stopReason == TaskStopReason.OwnerDisposed )
                break;

            try
            {
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
                        break;

                    queue.QueueProcessor.SignalContinuation();
                }

                await Task.Delay( TimeSpan.FromSeconds( 1 ) ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                queue.Emit( MessageBrokerQueueEvent.Unexpected( queue, exc ) );
            }
        }
    }

    internal void Dispose()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( false );
    }

    internal void SetUnderlyingTask(Task task)
    {
        Assume.IsNull( _task );
        _task = task;
    }

    internal Task? DiscardUnderlyingTask()
    {
        var result = _task;
        _task = null;
        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SignalContinuation()
    {
        if ( _continuation.Status == ValueTaskSourceStatus.Pending )
            _continuation.SetResult( true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerQueue queue)
    {
        var buffer = ListSlim<QueueMessage>.Create( minCapacity: 16 );
        while ( true )
        {
            var @continue = await queue.QueueProcessor._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            var disposed = false;
            while ( true )
            {
                int dequeued;
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    dequeued = queue.CopyMessagesIntoUnsafe( ref buffer );
                    if ( dequeued == 0 )
                    {
                        if ( queue.TryDisposeDueToPotentiallyEmptyQueueUnsafe() )
                            disposed = true;
                        else
                            queue.QueueProcessor._continuation.Reset();

                        break;
                    }
                }

                try
                {
                    MessageNotifications.SendMessages( queue.Client, in buffer );
                }
                catch ( Exception exc )
                {
                    // TODO: log failure to enqueue notifications & continue (log refactor)
                }

                using ( queue.AcquireLock() )
                    queue.DequeueMessagesUnsafe( dequeued );

                ClearBuffer( queue, ref buffer );
            }

            if ( disposed )
            {
                await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: true ).ConfigureAwait( false );
                return TaskStopReason.OwnerDisposed;
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ClearBuffer(MessageBrokerQueue queue, ref ListSlim<QueueMessage> messages)
    {
        foreach ( ref readonly var message in messages )
            queue.Emit( MessageBrokerQueueEvent.MessageDequeued( queue, message.Listener, message.Id ) );

        messages.Clear();
    }
}
