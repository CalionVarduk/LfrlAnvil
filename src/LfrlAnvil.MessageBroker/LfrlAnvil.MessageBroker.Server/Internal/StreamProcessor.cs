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
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Sources;
using LfrlAnvil.Async;
using LfrlAnvil.Exceptions;
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server.Internal;

internal struct StreamProcessor
{
    private readonly ManualResetValueTaskSource<bool> _continuation;
    private Task? _task;

    private StreamProcessor(ManualResetValueTaskSource<bool> continuation)
    {
        _continuation = continuation;
        _task = null;
    }

    [Pure]
    internal static StreamProcessor Create()
    {
        return new StreamProcessor( new ManualResetValueTaskSource<bool>() );
    }

    [MethodImpl( MethodImplOptions.NoInlining )]
    internal static async Task StartUnderlyingTask(MessageBrokerStream stream)
    {
        while ( true )
        {
            TaskStopReason stopReason;
            try
            {
                stopReason = await RunCore( stream ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                stream.Emit( MessageBrokerStreamEvent.Unexpected( stream, exc ) );
                stopReason = TaskStopReason.Error;
            }

            if ( stopReason == TaskStopReason.OwnerDisposed )
                break;

            try
            {
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                        break;

                    stream.StreamProcessor.SignalContinuation();
                }

                await Task.Delay( TimeSpan.FromSeconds( 1 ) ).ConfigureAwait( false );
            }
            catch ( Exception exc )
            {
                stream.Emit( MessageBrokerStreamEvent.Unexpected( stream, exc ) );
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
    private static async ValueTask<TaskStopReason> RunCore(MessageBrokerStream stream)
    {
        var buffer = ListSlim<StreamMessage>.Create( minCapacity: 16 );
        while ( true )
        {
            var @continue = await stream.StreamProcessor._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return TaskStopReason.OwnerDisposed;

            var disposed = false;
            while ( true )
            {
                MessageBrokerChannel? channel;
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    channel = stream.CopyMessagesIntoUnsafe( ref buffer );
                    if ( channel is null )
                    {
                        if ( stream.TryDisposeDueToEmptyQueueUnsafe() )
                            disposed = true;
                        else
                            stream.StreamProcessor._continuation.Reset();

                        break;
                    }
                }

                var listeners = channel.Listeners.GetAll();
                using ( stream.AcquireLock() )
                {
                    if ( stream.ShouldCancel )
                        return TaskStopReason.OwnerDisposed;

                    var exceptions = Chain<Exception>.Empty;
                    foreach ( var listener in listeners )
                    {
                        try
                        {
                            listener.Queue.PushMessages( listener, in buffer );
                        }
                        catch ( Exception exc )
                        {
                            exceptions = exceptions.Extend( exc );
                        }
                    }

                    if ( exceptions.Count > 0 )
                        ExceptionThrower.Throw( exceptions.Count == 1 ? exceptions.First() : new AggregateException( exceptions ) );

                    stream.DequeueMessagesUnsafe( buffer.Count );
                }

                ClearBuffer( stream, ref buffer );
            }

            if ( disposed )
            {
                await stream.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: true ).ConfigureAwait( false );
                return TaskStopReason.OwnerDisposed;
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ClearBuffer(MessageBrokerStream stream, ref ListSlim<StreamMessage> messages)
    {
        foreach ( ref readonly var message in messages )
        {
            stream.Emit( MessageBrokerStreamEvent.MessageDequeued( stream, message.Publisher, message.Id ) );
            message.Return();
        }

        messages.Clear();
    }
}
