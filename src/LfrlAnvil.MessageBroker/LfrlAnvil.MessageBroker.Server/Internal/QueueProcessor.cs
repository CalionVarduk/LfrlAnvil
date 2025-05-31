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
using LfrlAnvil.MessageBroker.Server.Exceptions;

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
            try
            {
                await RunCore( queue ).ConfigureAwait( false );
                break;
            }
            catch ( Exception exc )
            {
                ulong traceId;
                using ( queue.AcquireLock() )
                    traceId = queue.GetTraceId();

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, traceId, MessageBrokerQueueTraceEventType.Unexpected ) )
                {
                    MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ).Emit( queue.Logger.Error );
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
                    catch ( Exception exc2 )
                    {
                        MessageBrokerQueueErrorEvent.Create( queue, traceId, exc2 ).Emit( queue.Logger.Error );
                    }
                }
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
    private static async ValueTask RunCore(MessageBrokerQueue queue)
    {
        var buffer = ListSlim<QueueMessage>.Create( minCapacity: 16 );
        while ( true )
        {
            var @continue = await queue.QueueProcessor._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            var disposed = false;
            while ( true )
            {
                int dequeued;
                ulong traceId;
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
                        return;

                    dequeued = queue.CopyMessagesIntoUnsafe( ref buffer );
                    if ( dequeued == 0 )
                    {
                        if ( queue.TryDisposeDueToPotentiallyEmptyQueueUnsafe() )
                            disposed = true;
                        else
                            queue.QueueProcessor._continuation.Reset();

                        break;
                    }

                    traceId = queue.GetTraceId();
                }

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, traceId, MessageBrokerQueueTraceEventType.ProcessMessages ) )
                {
                    MessageBrokerQueueProcessingMessagesEvent.Create( queue, traceId, buffer.Count, dequeued - buffer.Count )
                        .Emit( queue.Logger.ProcessingMessages );

                    if ( buffer.Count > 0 )
                    {
                        using ( AcquireActiveClientLock( queue, traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return;

                            MessageNotifications.EnqueueMessagesUnsafe( queue.Client, in buffer );
                            queue.Client.MessageNotifications.SignalContinuation();
                        }
                    }

                    using ( queue.AcquireLock() )
                        queue.DequeueMessagesUnsafe( dequeued );

                    ClearBuffer( queue, ref buffer, traceId );
                }
            }

            if ( disposed )
            {
                ulong traceId;
                using ( queue.AcquireLock() )
                    traceId = queue.GetTraceId();

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, traceId, MessageBrokerQueueTraceEventType.Dispose ) )
                    await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: true, traceId ).ConfigureAwait( false );

                return;
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void ClearBuffer(MessageBrokerQueue queue, ref ListSlim<QueueMessage> messages, ulong traceId)
    {
        foreach ( ref readonly var message in messages )
            MessageBrokerQueueMessageProcessedEvent.Create( message.Listener, traceId, message.Publisher, message.Id, message.Length )
                .Emit( queue.Logger.MessageProcessed );

        messages.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireActiveClientLock(
        MessageBrokerQueue queue,
        ulong traceId,
        out MessageBrokerRemoteClientDisposedException? exception)
    {
        var @lock = queue.Client.AcquireLock();
        if ( ! queue.Client.ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = new MessageBrokerRemoteClientDisposedException( queue.Client );
        MessageBrokerQueueErrorEvent.Create( queue, traceId, exception ).Emit( queue.Logger.Error );
        return default;
    }
}
