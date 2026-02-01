// Copyright 2025-2026 Łukasz Furlepa
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
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Internal;
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
    internal static QueueProcessor Create(bool running)
    {
        var source = new ManualResetValueTaskSource<bool>();
        if ( ! running )
            source.TrySetResult( false );

        return new QueueProcessor( source );
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
                    var error = queue.Logger.Error;
                    error?.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                    try
                    {
                        using ( queue.AcquireLock() )
                        {
                            if ( queue.IsInactive )
                                break;

                            queue.QueueProcessor.SignalContinuation();
                        }

                        await Task.Delay( TimeSpan.FromSeconds( 1 ) ).ConfigureAwait( false );
                    }
                    catch ( Exception exc2 )
                    {
                        error?.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc2 ) );
                    }
                }
            }
        }
    }

    internal void BeginDispose(ref Chain<Exception> exceptions)
    {
        try
        {
            _continuation.TrySetResult( false );
        }
        catch ( Exception exc )
        {
            exceptions = exceptions.Extend( exc );
        }
    }

    internal void SetUnderlyingTask(Task task)
    {
        Assume.IsNull( _task );
        _task = task;
    }

    [Pure]
    internal Task? GetUnderlyingTask()
    {
        return _task;
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
        _continuation.TrySetResult( true );
    }

    // TODO
    // initial non-ephemeral inactive queue implementation could simply hold everything in memory just like it is doing now
    // just don't have an active queue processor
    // and make sure that listener/queue state doesn't block stream processor from pushing messages to the queue
    //
    // later, this can be improved to work with an alternative queue processor which immediately pushes messages to an append-only log file
    // so nothing is stored in memory for disconnected clients
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
                var message = default( QueueMessage );
                var messageType = QueueMessageStore.MessageType.Pending;
                var retryNo = 0;
                var lastRedeliveryNo = 0;
                bool hasMessage;
                using ( queue.AcquireLock() )
                {
                    if ( queue.IsInactive )
                        return;

                    hasMessage = queue.MessageStore.TryPeekNext(
                        queue.Client.GetTimestamp(),
                        ref discardedBuffer,
                        ref message,
                        ref messageType,
                        ref retryNo,
                        ref lastRedeliveryNo );

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

                    var ackId = 0;
                    Int31BoolPair retry;
                    Int31BoolPair redelivery;
                    switch ( messageType )
                    {
                        case QueueMessageStore.MessageType.Redelivery:
                        {
                            if ( lastRedeliveryNo >= message.Listener.MaxRedeliveries )
                            {
                                var messageRemoved = false;
                                message.Listener.DecrementPrefetchCounter();
                                if ( message.Listener.DeadLetterCapacityHint > 0 )
                                {
                                    using ( queue.AcquireLock() )
                                    {
                                        queue.MessageStore.AddToDeadLetter( message, retryNo, lastRedeliveryNo );
                                        queue.MessageStore.Dequeue( QueueMessageStore.MessageType.Redelivery );
                                        message.Listener.IncrementDeadLetterCounter();
                                    }
                                }
                                else
                                {
                                    using ( queue.AcquireLock() )
                                        queue.MessageStore.Dequeue( QueueMessageStore.MessageType.Redelivery );

                                    messageRemoved = queue.RemoveFromStreamMessageStore( message, traceId );
                                }

                                if ( queue.Logger.MessageDiscarded is { } messageDiscarded )
                                    messageDiscarded.Emit(
                                        MessageBrokerQueueMessageDiscardedEvent.Create(
                                            message.Listener,
                                            traceId,
                                            message.Publisher,
                                            message.StoreKey,
                                            retryNo,
                                            lastRedeliveryNo,
                                            messageRemoved,
                                            message.Listener.DeadLetterCapacityHint > 0,
                                            MessageBrokerQueueDiscardMessageReason.MaxRedeliveriesReached ) );

                                continue;
                            }

                            retry = Int31BoolPair.GetData( retryNo );
                            redelivery = Int31BoolPair.GetActiveData( unchecked( lastRedeliveryNo + 1 ) );
                            break;
                        }
                        case QueueMessageStore.MessageType.Retry:
                            retry = Int31BoolPair.GetActiveData( retryNo );
                            redelivery = Int31BoolPair.GetData( lastRedeliveryNo );
                            break;
                        case QueueMessageStore.MessageType.DeadLetter:
                            ackId = -1;
                            retry = Int31BoolPair.GetData( retryNo );
                            redelivery = Int31BoolPair.GetData( lastRedeliveryNo );
                            break;
                        default:
                            retry = Int31BoolPair.GetData( 0 );
                            redelivery = Int31BoolPair.GetData( 0 );
                            break;
                    }

                    if ( queue.Logger.ProcessingMessage is { } processingMessage )
                        processingMessage.Emit(
                            MessageBrokerQueueProcessingMessageEvent.Create(
                                message.Listener,
                                traceId,
                                message.Publisher,
                                message.StoreKey,
                                ackId != 0,
                                retry,
                                redelivery ) );

                    bool messageDataExists;
                    StreamMessage streamMessage;
                    using ( message.Publisher.Stream.AcquireLock() )
                        messageDataExists = message.Publisher.Stream.MessageStore.TryGet( message.StoreKey, out streamMessage );

                    if ( ! messageDataExists )
                    {
                        message.Listener.DecrementPrefetchCounter();
                        using ( queue.AcquireLock() )
                            queue.MessageStore.Dequeue( messageType );

                        if ( queue.Logger.Error is { } error )
                        {
                            var exc = queue.Exception( Resources.MessageDataNotFound( message.Publisher.Stream, message.StoreKey ) );
                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                        }

                        continue;
                    }

                    if ( messageType == QueueMessageStore.MessageType.Pending
                        && message.Listener.FilterExpression is not null
                        && ! message.Listener.FilterMessage( in streamMessage, traceId ) )
                    {
                        message.Listener.DecrementPrefetchCounter();
                        using ( queue.AcquireLock() )
                            queue.MessageStore.Dequeue( QueueMessageStore.MessageType.Pending );

                        var messageRemoved = queue.RemoveFromStreamMessageStore( message, traceId );
                        if ( queue.Logger.MessageDiscarded is { } messageDiscarded )
                            messageDiscarded.Emit(
                                MessageBrokerQueueMessageDiscardedEvent.Create(
                                    message.Listener,
                                    traceId,
                                    message.Publisher,
                                    message.StoreKey,
                                    0,
                                    0,
                                    messageRemoved,
                                    false,
                                    MessageBrokerQueueDiscardMessageReason.FilteredOut ) );

                        continue;
                    }

                    var ackExpiresAt = Timestamp.Zero;
                    var failed = true;
                    var poolToken = MemoryPoolToken<byte>.Empty;
                    try
                    {
                        if ( ackId == 0 && message.Listener.AreAcksEnabled )
                        {
                            using ( queue.AcquireLock() )
                                ackId = queue.MessageStore.AddUnacked(
                                    message,
                                    streamMessage.Id,
                                    retryNo,
                                    redelivery.IntValue,
                                    out ackExpiresAt );
                        }

                        var header = new Protocol.MessageNotificationHeader(
                            ackId,
                            message.Publisher.Stream.Id,
                            retry,
                            redelivery,
                            streamMessage.Id,
                            message.Publisher.Channel.Id,
                            message.Publisher.ClientId,
                            streamMessage.PushedAt,
                            streamMessage.Data.Length );

                        var totalLength = Protocol.PacketHeader.Length + unchecked( ( int )header.Header.Payload );
                        Assume.IsLessThanOrEqualTo( totalLength, queue.Client.Server.MaxNetworkMessagePacketLength.Bytes );

                        var memoryPool = queue.Client.GetMemoryPool( totalLength );
                        poolToken = memoryPool.Rent( totalLength, streamMessage.PoolToken.Clear, out var data );
                        header.Serialize( data );
                        streamMessage.Data.CopyTo(
                            data.Slice( Protocol.PacketHeader.Length + Protocol.MessageNotificationHeader.Payload ) );

                        var requiresSenderName = false;
                        var requiresStreamName = false;
                        using ( AcquireActiveClientLock( queue, traceId, out var exc ) )
                        {
                            if ( exc is not null )
                                return;

                            var enqueued = true;
                            var synchronizeExternalObjectNames = queue.Client.SynchronizeExternalObjectNames;
                            if ( synchronizeExternalObjectNames )
                            {
                                if ( queue.Client.ExternalNameCache.RequiresUpdate( message.Publisher ) )
                                {
                                    requiresSenderName = true;
                                    enqueued = false;
                                }

                                if ( queue.Client.ExternalNameCache.RequiresUpdate( message.Publisher.Stream ) )
                                {
                                    requiresStreamName = true;
                                    enqueued = false;
                                }
                            }

                            if ( enqueued )
                            {
                                NotificationSender.EnqueueMessageUnsafe(
                                    queue.Client,
                                    in message,
                                    retry,
                                    redelivery,
                                    streamMessage.Id,
                                    ackId,
                                    poolToken,
                                    data );

                                failed = false;
                                queue.Client.NotificationSender.SignalContinuation();
                            }
                        }

                        if ( failed )
                        {
                            var senderNamePoolToken = MemoryPoolToken<byte>.Empty;
                            var senderNameData = Memory<byte>.Empty;
                            var streamNamePoolToken = MemoryPoolToken<byte>.Empty;
                            var streamNameData = Memory<byte>.Empty;
                            try
                            {
                                if ( requiresSenderName )
                                {
                                    var notification = new Protocol.ObjectNameNotification(
                                        MessageBrokerSystemNotificationType.SenderName,
                                        message.Publisher.ClientId,
                                        message.Publisher.ClientName );

                                    senderNamePoolToken = queue.Client.MemoryPool.Rent(
                                        notification.Length,
                                        queue.Client.ClearBuffers,
                                        out senderNameData );

                                    notification.Serialize( senderNameData );
                                }

                                if ( requiresStreamName )
                                {
                                    var notification = new Protocol.ObjectNameNotification(
                                        MessageBrokerSystemNotificationType.StreamName,
                                        message.Publisher.Stream.Id,
                                        message.Publisher.Stream.Name );

                                    streamNamePoolToken = queue.Client.MemoryPool.Rent(
                                        notification.Length,
                                        queue.Client.ClearBuffers,
                                        out streamNameData );

                                    notification.Serialize( streamNameData );
                                }

                                var senderNameEnqueued = false;
                                var streamNameEnqueued = false;
                                using ( AcquireActiveClientLock( queue, traceId, out var exc ) )
                                {
                                    if ( exc is not null )
                                        return;

                                    if ( requiresSenderName )
                                    {
                                        senderNameEnqueued = NotificationSender.TryEnqueueSenderNameUnsafe(
                                            queue.Client,
                                            in message,
                                            ref senderNamePoolToken,
                                            senderNameData );
                                    }

                                    if ( requiresStreamName )
                                    {
                                        streamNameEnqueued = NotificationSender.TryEnqueueStreamNameUnsafe(
                                            queue.Client,
                                            in message,
                                            ref streamNamePoolToken,
                                            streamNameData );
                                    }

                                    NotificationSender.EnqueueMessageUnsafe(
                                        queue.Client,
                                        in message,
                                        retry,
                                        redelivery,
                                        streamMessage.Id,
                                        ackId,
                                        poolToken,
                                        data );

                                    failed = false;
                                    queue.Client.NotificationSender.SignalContinuation();
                                }

                                if ( requiresSenderName && ! senderNameEnqueued )
                                    Return( queue, traceId, senderNamePoolToken );

                                if ( requiresStreamName && ! streamNameEnqueued )
                                    Return( queue, traceId, streamNamePoolToken );
                            }
                            finally
                            {
                                if ( failed )
                                {
                                    Return( queue, traceId, senderNamePoolToken );
                                    Return( queue, traceId, streamNamePoolToken );
                                }
                            }
                        }
                    }
                    finally
                    {
                        if ( failed )
                        {
                            // TODO:
                            // one edge case to think about
                            // if non-ephemeral listener has prefetch hint X and client disconnects
                            // and that listener has X pending unACKed messages (will be redelivered on reconnect)
                            // and then client reconnects but reduces listeners prefetch hint to lower than X
                            // will redeliveries be consumed correctly?
                            //
                            // I think it will be fine, as long as listener prefetch counters correctly reflect
                            // initial number of unACKed queue messages related to them
                            //
                            // the only issue is that the lowered prefetch hint will be initially ignored
                            // due to larger number of messages to redeliver, which won't wait until prefetch counter is released enough
                            // I think that's acceptable, client seems to be ok with it, there is no prefetch validation
                            //
                            // however, a test that verifies that would be useful
                            //
                            // there is another, probably more important issue
                            // retry & redelivery limits on the listener
                            // if listener is reactivated with lower limits but some messages are pending/retrying etc.
                            // with larger retry/redelivery values than the new limits
                            // then it might end up not working - some assumptions or actual validations may block it
                            // solution: respect old limits one last time and send those messages anyway
                            // if client is no longer interested, then they can send a proper NACK

                            // TODO: TESTS
                            // - listener starts with prefetch hint 2, 2 messages are sent to client, unacked
                            //   client disconnects, inactive listener receives another message
                            //   client reconnects, then binds the same listener with 1 prefetch hint
                            //   should send both unacked messages immediately, should send third message only when both get acked
                            // - listener starts with max-redelivery = max, single message gets sent without acks 3 times
                            //   client disconnects, client reconnects, binds the same listener with max-redelivery = 1
                            //   message should be sent one last time, even though redelivery count exceeds max
                            // - same with max-retries
                            // - what about dead letter count? they might get marked as exceeded immediately on listener rebind, which is fine
                            if ( messageType != QueueMessageStore.MessageType.Redelivery )
                                message.Listener.DecrementPrefetchCounter();

                            if ( ackId > 0 )
                            {
                                using ( queue.AcquireLock() )
                                    queue.MessageStore.RemoveUnacked( ackId );
                            }

                            Return( queue, traceId, poolToken );
                        }
                        else
                        {
                            using ( queue.AcquireLock() )
                                queue.MessageStore.Dequeue( messageType );

                            var messageRemoved = ackId <= 0 && queue.RemoveFromStreamMessageStore( message, traceId );
                            if ( queue.Logger.MessageProcessed is { } messageProcessed )
                                messageProcessed.Emit(
                                    MessageBrokerQueueMessageProcessedEvent.Create(
                                        message.Listener,
                                        traceId,
                                        message.Publisher,
                                        streamMessage.Id,
                                        ackId,
                                        messageRemoved,
                                        ackExpiresAt ) );
                        }
                    }
                }
            }

            if ( disposed )
            {
                ulong traceId;
                using ( queue.AcquireLock() )
                    traceId = queue.GetTraceId();

                using ( MessageBrokerQueueTraceEvent.CreateScope( queue, traceId, MessageBrokerQueueTraceEventType.Deactivate ) )
                    await queue.DisposeDueToLackOfReferencesAsync( ignoreProcessorTask: true, traceId ).ConfigureAwait( false );

                return;
            }

            using ( queue.Client.AcquireLock() )
            using ( queue.AcquireLock() )
            {
                if ( ! queue.Client.IsInactive )
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
                var messageRemoved = queue.RemoveFromStreamMessageStore(
                    new QueueMessage( message.Publisher, message.Listener, message.StoreKey ),
                    traceId );

                if ( queue.Logger.MessageDiscarded is { } messageDiscarded )
                    messageDiscarded.Emit(
                        MessageBrokerQueueMessageDiscardedEvent.Create(
                            message.Listener,
                            traceId,
                            message.Publisher,
                            message.StoreKey,
                            message.Retry,
                            message.Redelivery,
                            messageRemoved,
                            false,
                            message.Reason ) );
            }
            catch ( Exception exc )
            {
                if ( queue.Logger.Error is { } error )
                    error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
            }
        }

        messages.Clear();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static ExclusiveLock AcquireActiveClientLock(
        MessageBrokerQueue queue,
        ulong traceId,
        out MessageBrokerRemoteClientDeactivatedException? exception)
    {
        var @lock = queue.Client.AcquireLock();
        if ( ! queue.Client.IsInactive )
        {
            exception = null;
            return @lock;
        }

        var disposed = queue.Client.IsDisposed;
        @lock.Dispose();
        exception = queue.Client.DeactivatedException( disposed );
        if ( queue.Logger.Error is { } error )
            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void Return(MessageBrokerQueue queue, ulong traceId, MemoryPoolToken<byte> poolToken)
    {
        var exc = poolToken.Return();
        if ( exc is not null && queue.Logger.Error is { } error )
            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
    }
}
