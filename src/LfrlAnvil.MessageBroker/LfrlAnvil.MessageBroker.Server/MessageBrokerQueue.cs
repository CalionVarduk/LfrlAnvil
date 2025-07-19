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

using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker queue, which allows a single <see cref="MessageBrokerRemoteClient"/> instance to manage
/// the order of message notifications between multiple listeners, sent by the server.
/// </summary>
public sealed class MessageBrokerQueue
{
    internal ReferenceStore<int, MessageBrokerChannelListenerBinding> ListenersByChannelId;
    internal QueueProcessor QueueProcessor;
    internal QueueMessageStore MessageStore;
    internal readonly MessageBrokerQueueLogger Logger;
    internal int EventHeapIndex;
    internal int DeadLetterQueryCounter;
    private MessageBrokerQueueState _state;
    private readonly object _sync = new object();
    private ulong _nextTraceId;

    internal MessageBrokerQueue(MessageBrokerRemoteClient client, int id, string name)
    {
        Client = client;
        Id = id;
        Name = name;
        EventHeapIndex = -1;
        DeadLetterQueryCounter = 0;
        _state = MessageBrokerQueueState.Running;
        _nextTraceId = 0;
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        MessageStore = QueueMessageStore.Create();
        QueueProcessor = QueueProcessor.Create();
        Logger = Client.Server.QueueLoggerFactory?.Invoke( this ) ?? default;
        QueueProcessor.SetUnderlyingTask( QueueProcessor.StartUnderlyingTask( this ) );
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this queue belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// Queue's unique identifier assigned by the client.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Queue's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current queue's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerQueueState"/> for more information.</remarks>
    public MessageBrokerQueueState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelListenerBinding"/> instances attached to this queue, identified by channel ids.
    /// </summary>
    public MessageBrokerQueueListenerCollection Listeners => new MessageBrokerQueueListenerCollection( this );

    /// <summary>
    /// Collection of messages stored in this queue.
    /// </summary>
    public MessageBrokerQueueMessageCollection Messages => new MessageBrokerQueueMessageCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerQueueState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueue"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' queue ({State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool PushMessage(
        MessageBrokerChannelListenerBinding listener,
        int storeKey,
        in StreamMessage message,
        MessageBrokerStream stream,
        ulong streamTraceId)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return false;

            traceId = GetTraceId();
        }

        using ( MessageBrokerQueueTraceEvent.CreateScope( this, traceId, MessageBrokerQueueTraceEventType.EnqueueMessage ) )
        {
            if ( Logger.StreamTrace is { } streamTrace )
                streamTrace.Emit( MessageBrokerQueueStreamTraceEvent.Create( this, traceId, stream, streamTraceId ) );

            if ( Logger.EnqueueingMessage is { } enqueueingMessage )
                enqueueingMessage.Emit(
                    MessageBrokerQueueEnqueueingMessageEvent.Create(
                        listener,
                        traceId,
                        message.Publisher,
                        message.Id,
                        storeKey,
                        message.Data.Length ) );

            using ( AcquireActiveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return false;

                MessageStore.Enqueue( message.Publisher, listener, storeKey );
                QueueProcessor.SignalContinuation();
            }

            if ( Logger.MessageEnqueued is { } messageEnqueued )
                messageEnqueued.Emit( MessageBrokerQueueMessageEnqueuedEvent.Create( listener, traceId, message.Publisher, message.Id ) );
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal AckResult HandleAck(
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        ref QueueMessage message,
        ref ulong traceId,
        ref bool disposing)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return AckResult.QueueDisposed;

            ref var entry = ref MessageStore.GetUnackedRef( ackId );
            if ( Unsafe.IsNullRef( ref entry ) || entry.Message.Publisher.Stream.Id != streamId || entry.MessageId != messageId )
                return AckResult.MessageNotFound;

            message = entry.Message;
            if ( entry.Retry != retry || entry.Redelivery != redelivery )
                return AckResult.MessageVersionNotFound;

            MessageStore.RemoveUnacked( ackId );
            disposing = TryDisposeDueToPotentiallyEmptyStoreUnsafe();
            if ( message.Listener.DecrementPrefetchCounter() && ! MessageStore.IsEmpty )
                QueueProcessor.SignalContinuation();

            traceId = GetTraceId();
        }

        return AckResult.Success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal AckResult HandleNegativeAck(
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        bool noRetry,
        bool noDeadLetter,
        Duration? explicitDelay,
        ref QueueMessage message,
        ref Duration delay,
        ref ulong traceId,
        ref bool disposing)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return AckResult.QueueDisposed;

            ref var info = ref MessageStore.GetUnackedRef( ackId );
            if ( Unsafe.IsNullRef( ref info ) || info.Message.Publisher.Stream.Id != streamId || info.MessageId != messageId )
                return AckResult.MessageNotFound;

            message = info.Message;
            if ( info.Retry != retry || info.Redelivery != redelivery )
                return AckResult.MessageVersionNotFound;

            if ( noRetry || retry >= message.Listener.MaxRetries )
            {
                var signalProcessor = message.Listener.DecrementPrefetchCounter();
                if ( ! noDeadLetter && message.Listener.DeadLetterCapacityHint > 0 )
                {
                    MessageStore.AddToDeadLetter( message, retry, redelivery );
                    MessageStore.RemoveUnacked( ackId );
                    signalProcessor |= message.Listener.IncrementDeadLetterCounter();
                }
                else
                {
                    MessageStore.RemoveUnacked( ackId );
                    disposing = TryDisposeDueToPotentiallyEmptyStoreUnsafe();
                    signalProcessor &= ! MessageStore.IsEmpty;
                }

                if ( signalProcessor )
                    QueueProcessor.SignalContinuation();
            }
            else
            {
                delay = explicitDelay ?? message.Listener.RetryDelay;
                MessageStore.ScheduleRetry( message, retry, redelivery, delay );
                MessageStore.RemoveUnacked( ackId );
                message.Listener.DecrementPrefetchCounter();
                QueueProcessor.SignalContinuation();
            }

            traceId = GetTraceId();
        }

        return AckResult.Success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal DeadLetterQueryResult HandleDeadLetterQuery(
        int readCount,
        ref int totalCount,
        ref int maxReadCount,
        ref Timestamp nextExpirationAt)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return DeadLetterQueryResult.QueueDisposed;

            totalCount = MessageStore.DeadLetter.Count;
            var desiredReadCount = unchecked( ( long )DeadLetterQueryCounter + readCount );
            maxReadCount = totalCount > desiredReadCount ? unchecked( ( int )desiredReadCount ) : totalCount;
            DeadLetterQueryCounter = maxReadCount;
            if ( totalCount > 0 )
            {
                ref var first = ref MessageStore.DeadLetter.First();
                nextExpirationAt = first.ExpiresAt;
            }

            if ( DeadLetterQueryCounter > 0 )
                QueueProcessor.SignalContinuation();
        }

        return DeadLetterQueryResult.Success;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool RemoveFromStreamMessageStore(QueueMessage message, ulong traceId)
    {
        bool removed;
        Result<bool> result;
        using ( message.Publisher.Stream.AcquireLock() )
        {
            result = message.Publisher.Stream.MessageStore.DecrementRefCount(
                message.StoreKey,
                out removed );

            if ( removed && message.Publisher.Stream.MessageStore.IsEmpty && ! message.Publisher.Stream.ShouldCancel )
                message.Publisher.Stream.StreamProcessor.SignalContinuation();
        }

        var error = Logger.Error;
        if ( result.Exception is not null )
            error?.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, result.Exception ) );

        if ( ! result.Value && error is not null )
        {
            var exc = new MessageBrokerQueueException(
                this,
                Resources.MessageDataNotFound( message.Publisher.Stream, message.StoreKey ) );

            error.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, exc ) );
        }

        return removed;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingListenerUnsafe(int channelId)
    {
        ListenersByChannelId.Remove( channelId );
        if ( ListenersByChannelId.Count > 0 || ! MessageStore.IsEmpty )
        {
            QueueProcessor.SignalContinuation();
            return false;
        }

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToPotentiallyEmptyStoreUnsafe()
    {
        if ( ListenersByChannelId.Count > 0 || ! MessageStore.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask OnClientDisconnectedAsync(ulong clientTraceId)
    {
        return DisposeAsync( clientTraceId, true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ValueTask OnServerDisposedAsync(ulong clientTraceId)
    {
        return DisposeAsync( clientTraceId, false );
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(bool ignoreProcessorTask, ulong traceId)
    {
        Assume.Equals( State, MessageBrokerQueueState.Disposing );
        if ( Logger.Disposing is { } disposing )
            disposing.Emit( MessageBrokerQueueDisposingEvent.Create( this, traceId ) );

        Task? processorTask;
        using ( AcquireLock() )
        {
            Assume.Equals( ListenersByChannelId.Count, 0 );
            Assume.True( MessageStore.IsEmpty );

            DeadLetterQueryCounter = 0;
            processorTask = QueueProcessor.DiscardUnderlyingTask();
            if ( ignoreProcessorTask )
                processorTask = null;

            QueueProcessor.Dispose();
        }

        if ( processorTask is not null )
            await processorTask.ConfigureAwait( false );

        using ( Client.AcquireLock() )
        {
            if ( ! Client.ShouldCancel )
            {
                Client.QueueStore.Remove( Id, Name );
                Client.EventScheduler.RemoveQueue( this );
            }
        }

        using ( AcquireLock() )
            _state = MessageBrokerQueueState.Disposed;

        if ( Logger.Disposed is { } disposed )
            disposed.Emit( MessageBrokerQueueDisposedEvent.Create( this, traceId ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireActiveLock(ulong traceId, out MessageBrokerQueueDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! ShouldCancel )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = new MessageBrokerQueueDisposedException( this );
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    private async ValueTask DisposeAsync(ulong clientTraceId, bool decrementMessageRefCount)
    {
        ulong traceId;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerQueueState.Disposing;
            traceId = GetTraceId();
        }

        using ( MessageBrokerQueueTraceEvent.CreateScope( this, traceId, MessageBrokerQueueTraceEventType.Dispose ) )
        {
            if ( Logger.ClientTrace is { } clientTrace )
                clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( this, traceId, clientTraceId ) );

            if ( Logger.Disposing is { } disposing )
                disposing.Emit( MessageBrokerQueueDisposingEvent.Create( this, traceId ) );

            Task? processorTask;
            using ( AcquireLock() )
            {
                DeadLetterQueryCounter = 0;
                ListenersByChannelId.Clear();
                processorTask = QueueProcessor.DiscardUnderlyingTask();
                QueueProcessor.Dispose();
            }

            if ( processorTask is not null )
                await processorTask.ConfigureAwait( false );

            int discardedMessageCount;
            if ( decrementMessageRefCount )
                discardedMessageCount = QueueMessageStore.ClearAndRelease( this, traceId );
            else
            {
                using ( AcquireLock() )
                    discardedMessageCount = MessageStore.Clear();
            }

            if ( discardedMessageCount > 0 && Logger.Error is { } error )
            {
                var exc = new MessageBrokerQueueException( this, Resources.QueueMessagesDiscarded( discardedMessageCount ) );
                error.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, exc ) );
            }

            using ( AcquireLock() )
                _state = MessageBrokerQueueState.Disposed;

            if ( Logger.Disposed is { } disposed )
                disposed.Emit( MessageBrokerQueueDisposedEvent.Create( this, traceId ) );
        }
    }
}
