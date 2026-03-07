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
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Extensions;
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
    internal readonly MessageBrokerQueueLogger Logger;
    internal ReferenceStore<int, MessageBrokerChannelListenerBinding> ListenersByChannelId;
    internal QueueProcessor QueueProcessor;
    internal QueueMessageStore MessageStore;
    internal int EventHeapIndex;
    internal int DeadLetterQueryCounter;
    private readonly object _sync = new object();
    private ServerStorage.Client.Queue _storage;
    private MessageBrokerQueueState _state;
    private TaskCompletionSource? _deactivated;
    private ulong _nextTraceId;
    private ulong? _autoStopTraceId;

    private MessageBrokerQueue(
        MessageBrokerRemoteClient client,
        ServerStorage.Client.Queue storage,
        int id,
        string name,
        ulong nextTraceId,
        MessageBrokerQueueState state)
    {
        _storage = storage;
        Client = client;
        Id = id;
        Name = name;
        EventHeapIndex = -1;
        DeadLetterQueryCounter = 0;
        _state = state;
        _nextTraceId = nextTraceId;
        _autoStopTraceId = null;
        ListenersByChannelId = ReferenceStore<int, MessageBrokerChannelListenerBinding>.Create();
        MessageStore = QueueMessageStore.Create();
        QueueProcessor = QueueProcessor.Create( state == MessageBrokerQueueState.Running );
        Logger = Client.Server.QueueLoggerFactory?.Invoke( this ) ?? default;
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

    internal bool IsInactive => _state >= MessageBrokerQueueState.Deactivating;
    internal bool IsDisposed => _state >= MessageBrokerQueueState.Disposing;

    internal ServerStorage.Client.Queue Storage
    {
        get
        {
            using ( AcquireLock() )
                return _storage;
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueue"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' queue ({State})";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueue Create(MessageBrokerRemoteClient client, ServerStorage.Client.Queue storage, int id, string name)
    {
        return new MessageBrokerQueue( client, storage, id, name, nextTraceId: 0, MessageBrokerQueueState.Running );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueue CreateInactive(
        MessageBrokerRemoteClient client,
        ServerStorage.Client.Queue storage,
        int id,
        string name,
        ulong nextTraceId)
    {
        return new MessageBrokerQueue( client, storage, id, name, nextTraceId, MessageBrokerQueueState.Inactive );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsEphemeral()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueState.Inactive )
                _storage = default;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Reactivate()
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerQueueState.Inactive )
                return;

            _state = MessageBrokerQueueState.Running;
            QueueProcessor.ResetSignal();
            Client.EventScheduler.AddQueue( this );
            Client.EventScheduler.UpdateQueue( this );

            if ( QueueProcessor.GetUnderlyingTask() is null )
                QueueProcessor.SetUnderlyingTask( QueueProcessor.StartUnderlyingTask( this ) );

            QueueProcessor.SignalContinuation();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void StartProcessor()
    {
        using ( AcquireLock() )
        {
            if ( IsInactive )
                return;

            if ( QueueProcessor.GetUnderlyingTask() is null )
                QueueProcessor.SetUnderlyingTask( QueueProcessor.StartUnderlyingTask( this ) );

            QueueProcessor.SignalContinuation();
        }
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
            if ( IsDisposed )
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

            using ( AcquireAliveLock( traceId, out var exc ) )
            {
                if ( exc is not null )
                    return false;

                MessageStore.Enqueue( message.Publisher, listener, storeKey );
                if ( ! IsInactive )
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
            if ( IsInactive )
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
            if ( IsInactive )
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
            if ( IsInactive )
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

            if ( removed && message.Publisher.Stream.MessageStore.IsEmpty && ! message.Publisher.Stream.IsDisposed )
                message.Publisher.Stream.StreamProcessor.SignalContinuation();
        }

        var error = Logger.Error;
        if ( result.Exception is not null )
            error?.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, result.Exception ) );

        if ( ! result.Value && error is not null )
        {
            var exc = this.Exception( Resources.MessageDataNotFound( message.Publisher.Stream, message.StoreKey ) );
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
        _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeDueToPotentiallyEmptyStoreUnsafe()
    {
        if ( ListenersByChannelId.Count > 0 || ! MessageStore.IsEmpty )
            return false;

        _state = MessageBrokerQueueState.Disposing;
        _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal async ValueTask OnServerDisposingAsync(ulong clientTraceId)
    {
        var traceId = 0UL;
        MessageBrokerQueueState state;
        var exceptions = Chain<Exception>.Empty;
        Task? processorTask = null;

        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueState.Disposed )
                return;

            state = _state;
            if ( _state == MessageBrokerQueueState.Disposing )
            {
                processorTask = QueueProcessor.DiscardUnderlyingTask();
                QueueProcessor.BeginDispose( ref exceptions );
            }
            else
            {
                _state = MessageBrokerQueueState.Disposing;
                traceId = GetTraceId();
                _autoStopTraceId = traceId;
                DeadLetterQueryCounter = 0;
                _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            }
        }

        if ( state == MessageBrokerQueueState.Disposing )
        {
            Client.EmitErrors( ref exceptions, clientTraceId );
            Client.EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), clientTraceId );
            return;
        }

        if ( Logger.TraceStart is { } traceStart )
            traceStart.Emit( MessageBrokerQueueTraceEvent.Create( this, traceId, MessageBrokerQueueTraceEventType.Deactivate ) );

        if ( Logger.ClientTrace is { } clientTrace )
            clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( this, traceId, clientTraceId ) );

        if ( Logger.Deactivating is { } deactivating )
            deactivating.Emit( MessageBrokerQueueDeactivatingEvent.Create( this, traceId, isAlive: false ) );

        using ( AcquireLock() )
        {
            processorTask = QueueProcessor.DiscardUnderlyingTask();
            QueueProcessor.BeginDispose( ref exceptions );
        }

        EmitErrors( ref exceptions, traceId );
        EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
    }

    internal async ValueTask OnClientDeactivatingAsync(bool keepAlive, ulong clientTraceId)
    {
        var traceId = 0UL;
        MessageBrokerQueueState state;
        var exceptions = Chain<Exception>.Empty;
        Task? processorTask = null;

        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerQueueState.Disposed )
                return;

            state = _state;
            if ( _state == MessageBrokerQueueState.Disposing )
            {
                processorTask = QueueProcessor.DiscardUnderlyingTask();
                QueueProcessor.BeginDispose( ref exceptions );
            }
            else
            {
                if ( _state == MessageBrokerQueueState.Inactive && keepAlive )
                    return;

                _state = keepAlive ? MessageBrokerQueueState.Deactivating : MessageBrokerQueueState.Disposing;
                traceId = GetTraceId();
                _autoStopTraceId = traceId;
                DeadLetterQueryCounter = 0;
                _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            }
        }

        if ( state == MessageBrokerQueueState.Disposing )
        {
            Client.EmitErrors( ref exceptions, clientTraceId );
            Client.EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), clientTraceId );
            return;
        }

        if ( Logger.TraceStart is { } traceStart )
            traceStart.Emit( MessageBrokerQueueTraceEvent.Create( this, traceId, MessageBrokerQueueTraceEventType.Deactivate ) );

        if ( Logger.ClientTrace is { } clientTrace )
            clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( this, traceId, clientTraceId ) );

        if ( Logger.Deactivating is { } deactivating )
            deactivating.Emit( MessageBrokerQueueDeactivatingEvent.Create( this, traceId, keepAlive ) );

        using ( AcquireLock() )
        {
            processorTask = QueueProcessor.DiscardUnderlyingTask();
            QueueProcessor.BeginDispose( ref exceptions );
        }

        EmitErrors( ref exceptions, traceId );
        EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
    }

    internal async ValueTask OnServerDisposedAsync(
        bool isEphemeral,
        bool clearBuffers,
        Dictionary<int, MessageBrokerChannelListenerBinding> listenersByChannelId,
        bool storageLoaded,
        ulong clientTraceId)
    {
        ulong? autoDisposalTraceId;
        MessageBrokerQueueState state;
        TaskCompletionSource? deactivatedSource;
        ServerStorage.Client.Queue storage;

        using ( AcquireLock() )
        {
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerQueueState.Disposing );
            state = _state;
            autoDisposalTraceId = _autoStopTraceId;
            deactivatedSource = _deactivated;
            storage = _storage;
        }

        if ( autoDisposalTraceId is null )
        {
            if ( state == MessageBrokerQueueState.Disposing )
            {
                if ( storageLoaded )
                    Client.EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

                Client.EmitError( await (deactivatedSource?.Task).AsSafeCancellable().ConfigureAwait( false ), clientTraceId );
            }

            return;
        }

        try
        {
            QueueMessageStore.ReleaseMessages(
                this,
                autoDisposalTraceId.Value,
                extractPersistentMessages: ! isEphemeral,
                discardAllMessages: false,
                listenersByChannelId,
                out var pendingMessages,
                out var unackedEntries,
                out var retryEntries,
                out var deadLetterEntries );

            if ( ! isEphemeral )
            {
                bool hasListeners;
                using ( AcquireLock() )
                    hasListeners = ListenersByChannelId.Count > 0;

                if ( ! hasListeners
                    && pendingMessages.IsEmpty
                    && unackedEntries.IsEmpty
                    && retryEntries.IsEmpty
                    && deadLetterEntries.IsEmpty
                    && storageLoaded )
                    EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), autoDisposalTraceId.Value );
                else
                {
                    EmitError(
                        await storage.SaveMetadataAsync( this, clearBuffers, autoDisposalTraceId.Value )
                            .AsSafe()
                            .ConfigureAwait( false ),
                        autoDisposalTraceId.Value );

                    if ( storageLoaded )
                    {
                        EmitError(
                            await storage.SaveAsync( this, pendingMessages, clearBuffers, autoDisposalTraceId.Value )
                                .AsSafe()
                                .ConfigureAwait( false ),
                            autoDisposalTraceId.Value );

                        EmitError(
                            await storage.SaveAsync( this, unackedEntries, clearBuffers, autoDisposalTraceId.Value )
                                .AsSafe()
                                .ConfigureAwait( false ),
                            autoDisposalTraceId.Value );

                        EmitError(
                            await storage.SaveAsync( this, retryEntries, clearBuffers, autoDisposalTraceId.Value )
                                .AsSafe()
                                .ConfigureAwait( false ),
                            autoDisposalTraceId.Value );

                        EmitError(
                            await storage.SaveAsync( this, deadLetterEntries, clearBuffers, autoDisposalTraceId.Value )
                                .AsSafe()
                                .ConfigureAwait( false ),
                            autoDisposalTraceId.Value );
                    }
                }
            }

            using ( AcquireLock() )
            {
                ListenersByChannelId.Clear();
                MessageStore.Clear();
                _state = MessageBrokerQueueState.Disposed;
                _autoStopTraceId = null;
                _deactivated = null;
            }

            if ( Logger.Deactivated is { } deactivated )
                deactivated.Emit( MessageBrokerQueueDeactivatedEvent.Create( this, autoDisposalTraceId.Value, isAlive: false ) );
        }
        finally
        {
            if ( Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit(
                    MessageBrokerQueueTraceEvent.Create( this, autoDisposalTraceId.Value, MessageBrokerQueueTraceEventType.Deactivate ) );

            deactivatedSource?.TrySetResult();
        }
    }

    internal async ValueTask OnClientDeactivatedAsync(bool keepAlive, ulong clientTraceId)
    {
        ulong? autoDisposalTraceId;
        MessageBrokerQueueState state;
        bool dispose;
        TaskCompletionSource? deactivatedSource;
        ServerStorage.Client.Queue storage;

        using ( AcquireLock() )
        {
            Assume.IsGreaterThanOrEqualTo( _state, MessageBrokerQueueState.Deactivating );
            state = _state;
            autoDisposalTraceId = _autoStopTraceId;
            dispose = ListenersByChannelId.Count == 0 && MessageStore.IsEmpty;
            if ( dispose )
                _state = MessageBrokerQueueState.Disposing;

            deactivatedSource = _deactivated;
            storage = _storage;
        }

        if ( autoDisposalTraceId is null )
        {
            if ( state == MessageBrokerQueueState.Disposing )
            {
                Client.EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );
                Client.EmitError( await (deactivatedSource?.Task).AsSafeCancellable().ConfigureAwait( false ), clientTraceId );
            }

            return;
        }

        try
        {
            if ( keepAlive )
            {
                // TODO
                // what if storage handling were to be added? there should be no overlap with server disposal/client delete
                // so should be safe...? as long as queue processor is fully deactivated
                // but this is another problem to solve: queue's message store must be swapped from in-memory to on-disk
                // in one lock, also must make sure that the order of messages is preserved during the swap
                // so the swap might have to happen inside storage async mutex lock?

                if ( dispose )
                {
                    using ( Client.AcquireLock() )
                        Client.QueueStore.Remove( Id, Name );

                    EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), autoDisposalTraceId.Value );

                    using ( AcquireLock() )
                    {
                        ListenersByChannelId.Clear();
                        MessageStore.Clear();
                        _state = MessageBrokerQueueState.Disposed;
                        _autoStopTraceId = null;
                        _deactivated = null;
                    }
                }
                else
                {
                    using ( AcquireLock() )
                    {
                        MessageStore.ExpireAllUnacked( Client.GetTimestamp() );
                        _state = MessageBrokerQueueState.Inactive;
                        _autoStopTraceId = null;
                        _deactivated = null;
                    }
                }
            }
            else
            {
                dispose = true;
                QueueMessageStore.ReleaseMessages(
                    this,
                    autoDisposalTraceId.Value,
                    extractPersistentMessages: false,
                    discardAllMessages: true,
                    listenersByChannelId: null,
                    out _,
                    out _,
                    out _,
                    out _ );

                EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), autoDisposalTraceId.Value );

                using ( AcquireLock() )
                {
                    ListenersByChannelId.Clear();
                    MessageStore.Clear();
                    _state = MessageBrokerQueueState.Disposed;
                    _autoStopTraceId = null;
                    _deactivated = null;
                }
            }

            if ( Logger.Deactivated is { } deactivated )
                deactivated.Emit( MessageBrokerQueueDeactivatedEvent.Create( this, autoDisposalTraceId.Value, ! dispose ) );
        }
        finally
        {
            if ( Logger.TraceEnd is { } traceEnd )
                traceEnd.Emit(
                    MessageBrokerQueueTraceEvent.Create( this, autoDisposalTraceId.Value, MessageBrokerQueueTraceEventType.Deactivate ) );

            deactivatedSource?.TrySetResult();
        }
    }

    internal async ValueTask DisposeDueToLackOfReferencesAsync(ulong traceId, bool ignoreProcessorTask = false)
    {
        TaskCompletionSource? deactivatedSource = null;
        try
        {
            Assume.Equals( State, MessageBrokerQueueState.Disposing );
            if ( Logger.Deactivating is { } deactivating )
                deactivating.Emit( MessageBrokerQueueDeactivatingEvent.Create( this, traceId, isAlive: false ) );

            ServerStorage.Client.Queue storage;
            var exceptions = Chain<Exception>.Empty;
            Task? processorTask;

            using ( AcquireLock() )
            {
                Assume.Equals( ListenersByChannelId.Count, 0 );
                Assume.True( MessageStore.IsEmpty );

                DeadLetterQueryCounter = 0;
                processorTask = QueueProcessor.GetUnderlyingTask();
                if ( ignoreProcessorTask )
                    processorTask = null;

                QueueProcessor.BeginDispose( ref exceptions );
                deactivatedSource = _deactivated;
                storage = _storage;
            }

            EmitErrors( ref exceptions, traceId );
            EmitError( await processorTask.AsSafeNonCancellable().ConfigureAwait( false ), traceId );
            EmitError( await storage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), traceId );

            using ( Client.AcquireLock() )
            {
                if ( ! Client.IsDisposed )
                {
                    Client.QueueStore.Remove( Id, Name );
                    using ( AcquireLock() )
                        Client.EventScheduler.RemoveQueue( this );
                }
            }

            using ( AcquireLock() )
            {
                _ = QueueProcessor.DiscardUnderlyingTask();
                _state = MessageBrokerQueueState.Disposed;
                _autoStopTraceId = null;
                _deactivated = null;
            }

            if ( Logger.Deactivated is { } deactivated )
                deactivated.Emit( MessageBrokerQueueDeactivatedEvent.Create( this, traceId, isAlive: false ) );
        }
        finally
        {
            deactivatedSource?.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ulong GetTraceId()
    {
        return unchecked( _nextTraceId++ );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EmitErrors(ref Chain<Exception> exceptions, ulong traceId)
    {
        if ( exceptions.Count > 0 && Logger.Error is { } error )
        {
            foreach ( var exc in exceptions )
                error.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, exc ) );
        }

        exceptions = Chain<Exception>.Empty;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireAliveLock(ulong traceId, out MessageBrokerQueueDisposedException? exception)
    {
        var @lock = AcquireLock();
        if ( ! IsDisposed )
        {
            exception = null;
            return @lock;
        }

        @lock.Dispose();
        exception = this.DisposedException();
        if ( Logger.Error is { } error )
            error.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, exception ) );

        return default;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void EmitError(Result result, ulong traceId)
    {
        if ( result.Exception is not null && Logger.Error is { } error )
            error.Emit( MessageBrokerQueueErrorEvent.Create( this, traceId, result.Exception ) );
    }
}
