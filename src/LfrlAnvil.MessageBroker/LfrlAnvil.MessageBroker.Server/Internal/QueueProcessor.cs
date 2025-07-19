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
                    var error = queue.Logger.Error;
                    error?.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
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
                        error?.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc2 ) );
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
                var message = default( QueueMessage );
                var messageType = QueueMessageStore.MessageType.Pending;
                var retryNo = 0;
                var lastRedeliveryNo = 0;
                bool hasMessage;
                using ( queue.AcquireLock() )
                {
                    if ( queue.ShouldCancel )
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
                            var exc = new MessageBrokerQueueException(
                                queue,
                                Resources.MessageDataNotFound( message.Publisher.Stream, message.StoreKey ) );

                            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
                        }

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
                    finally
                    {
                        if ( failed )
                        {
                            if ( messageType != QueueMessageStore.MessageType.Redelivery )
                                message.Listener.DecrementPrefetchCounter();

                            if ( ackId > 0 )
                            {
                                using ( queue.AcquireLock() )
                                    queue.MessageStore.RemoveUnacked( ackId );
                            }

                            var exc = poolToken.Return();
                            if ( exc is not null && queue.Logger.Error is { } error )
                                error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exc ) );
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
        if ( queue.Logger.Error is { } error )
            error.Emit( MessageBrokerQueueErrorEvent.Create( queue, traceId, exception ) );

        return default;
    }
}
