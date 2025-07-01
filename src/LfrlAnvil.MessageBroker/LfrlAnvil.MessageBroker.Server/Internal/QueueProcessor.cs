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
using LfrlAnvil.Chrono;
using LfrlAnvil.Memory;
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
        var discardedBuffer = ListSlim<QueueMessageStore.DiscardedMessage>.Create( minCapacity: 16 );
        while ( true )
        {
            var @continue = await queue.QueueProcessor._continuation.GetTask().ConfigureAwait( false );
            if ( ! @continue )
                return;

            var disposed = false;
            while ( true )
            {
                ulong traceId;
                QueueMessage message;
                QueueMessageStore.MessageType messageType;
                int retryAttempt;
                int lastRedeliveryAttempt;
                bool hasMessage;
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
                        return;

                    hasMessage = queue.MessageStore.TryPeekNext(
                        queue.Client.GetTimestamp(),
                        ref discardedBuffer,
                        out message,
                        out messageType,
                        out retryAttempt,
                        out lastRedeliveryAttempt );

                    if ( ! hasMessage && discardedBuffer.Count == 0 )
                    {
                        if ( queue.TryDisposeDueToPotentiallyEmptyStoreUnsafe() )
                            disposed = true;
                        else
                            queue.QueueProcessor._continuation.Reset();

                        break;
                    }

                    traceId = queue.GetTraceId();
                }

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, traceId, MessageBrokerQueueTraceEventType.ProcessMessage ) )
                {
                    DiscardMessages( queue, ref discardedBuffer, traceId );
                    if ( ! hasMessage )
                        continue;

                    ResendIndex retry;
                    ResendIndex redelivery;
                    switch ( messageType )
                    {
                        case QueueMessageStore.MessageType.Pending:
                            retry = ResendIndex.Create( 0 );
                            redelivery = ResendIndex.Create( 0 );
                            break;
                        case QueueMessageStore.MessageType.Retry:
                            retry = ResendIndex.CreateActive( retryAttempt );
                            redelivery = ResendIndex.Create( lastRedeliveryAttempt );
                            break;
                        default:
                        {
                            if ( lastRedeliveryAttempt >= message.Listener.MaxRedeliveries )
                            {
                                message.Listener.DecrementPrefetchCounter();
                                using ( queue.AcquireLock() )
                                    queue.MessageStore.Dequeue( QueueMessageStore.MessageType.Redelivery );

                                var messageDataRemoved = queue.RemoveFromStreamMessageStore( message, traceId );
                                MessageBrokerQueueMessageDiscardedEvent.Create(
                                        message.Listener,
                                        traceId,
                                        message.Publisher,
                                        message.StoreKey,
                                        retryAttempt,
                                        lastRedeliveryAttempt,
                                        messageDataRemoved,
                                        MessageBrokerQueueDiscardMessageReason.MaxRedeliveriesReached )
                                    .Emit( queue.Logger.MessageDiscarded );

                                // TODO: potential move to dead-letter
                                continue;
                            }

                            retry = ResendIndex.Create( retryAttempt );
                            redelivery = ResendIndex.CreateActive( unchecked( lastRedeliveryAttempt + 1 ) );
                            break;
                        }
                    }

                    MessageBrokerQueueProcessingMessageEvent.Create(
                            message.Listener,
                            traceId,
                            message.Publisher,
                            message.StoreKey,
                            retry,
                            redelivery )
                        .Emit( queue.Logger.ProcessingMessage );

                    bool messageDataExists;
                    StreamMessage streamMessage;
                    using ( message.Publisher.Stream.AcquireLock() )
                        messageDataExists = message.Publisher.Stream.MessageStore.TryGet( message.StoreKey, out streamMessage );

                    if ( ! messageDataExists )
                    {
                        using ( queue.AcquireLock() )
                            queue.MessageStore.Dequeue( messageType );

                        var error = new MessageBrokerQueueException(
                            queue,
                            Resources.MessageDataNotFound( message.Publisher.Stream, message.StoreKey ) );

                        MessageBrokerQueueErrorEvent.Create( queue, traceId, error ).Emit( queue.Logger.Error );
                        continue;
                    }

                    var ackId = 0;
                    var ackExpiresAt = Timestamp.Zero;
                    var failed = true;
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        if ( message.Listener.AreAcksEnabled )
                        {
                            using ( queue.AcquireLock() )
                                ackId = queue.MessageStore.AddUnacked(
                                    message,
                                    streamMessage.Id,
                                    retryAttempt,
                                    redelivery.Value,
                                    out ackExpiresAt );
                        }

                        var header = new Protocol.MessageNotificationHeader(
                            ackId,
                            message.Publisher.Stream.Id,
                            retry,
                            redelivery,
                            streamMessage.Id,
                            message.Publisher.Channel.Id,
                            message.Publisher.Client.Id,
                            streamMessage.PushedAt,
                            streamMessage.Data.Length );

                        poolToken = queue.Client.MemoryPool.Rent(
                                Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload + streamMessage.Data.Length,
                                out var data )
                            .EnableClearing( streamMessage.PoolToken.Clear );

                        header.Serialize( data.Slice( 0, Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload ) );
                        streamMessage.Data.CopyTo(
                            data.Slice( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload ) );

                        using ( AcquireActiveClientLock( queue, traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return;

                            MessageNotifications.EnqueueMessageUnsafe(
                                queue.Client,
                                in message,
                                retry,
                                redelivery,
                                streamMessage.Id,
                                ackId,
                                poolToken,
                                data );

                            failed = false;
                            queue.Client.MessageNotifications.SignalContinuation();
                        }
                    }
                    finally
                    {
                        if ( failed )
                        {
                            if ( ackId > 0 )
                            {
                                using ( queue.AcquireLock() )
                                    queue.MessageStore.RemoveUnacked( ackId );
                            }

                            var exc = poolToken.Return();
                            if ( exc is not null )
                                MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ).Emit( queue.Logger.Error );
                        }
                        else
                        {
                            using ( queue.AcquireLock() )
                                queue.MessageStore.Dequeue( messageType );

                            var messageDataRemoved = ackId == 0 && queue.RemoveFromStreamMessageStore( message, traceId );
                            MessageBrokerQueueMessageProcessedEvent.Create(
                                    message.Listener,
                                    traceId,
                                    message.Publisher,
                                    streamMessage.Id,
                                    ackId,
                                    ackExpiresAt,
                                    streamMessage.Data.Length,
                                    messageDataRemoved )
                                .Emit( queue.Logger.MessageProcessed );
                        }
                    }
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

            using ( queue.Client.AcquireLock() )
            using ( queue.AcquireLock() )
            {
                if ( ! queue.ShouldCancel )
                    queue.Client.EventScheduler.UpdateQueue( queue );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void DiscardMessages(MessageBrokerQueue queue, ref ListSlim<QueueMessageStore.DiscardedMessage> messages, ulong traceId)
    {
        foreach ( ref readonly var message in messages )
        {
            try
            {
                var messageDataRemoved = queue.RemoveFromStreamMessageStore(
                    new QueueMessage( message.Publisher, message.Listener, message.StoreKey ),
                    traceId );

                MessageBrokerQueueMessageDiscardedEvent.Create(
                        message.Listener,
                        traceId,
                        message.Publisher,
                        message.StoreKey,
                        message.Retry,
                        message.Redelivery,
                        messageDataRemoved,
                        message.Reason )
                    .Emit( queue.Logger.MessageDiscarded );
            }
            catch ( Exception exc )
            {
                MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ).Emit( queue.Logger.Error );
            }
        }

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
