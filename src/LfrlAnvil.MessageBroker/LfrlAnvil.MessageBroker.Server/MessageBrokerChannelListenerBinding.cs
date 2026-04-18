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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Extensions;
using LfrlAnvil.Memory;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Exceptions;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel binding for a listener, which allows clients to listen to messages published through channels
/// via queue bindings.
/// </summary>
public sealed class MessageBrokerChannelListenerBinding
{
    internal QueueBindingCollection QueueBindingCollection;
    private readonly object _sync = new object();

    private TaskCompletionSource? _deactivated;

    private string? _filterExpression;
    private int _prefetchHint;
    private int _maxRetries;
    private int _maxRedeliveries;
    private int _deadLetterCapacityHint;
    private Duration _retryDelay;
    private Duration _minAckTimeout;
    private Duration _minDeadLetterRetention;
    private MessageBrokerChannelListenerBindingState _state;
    private bool _isEphemeral;
    private bool _autoDisposed;

    private MessageBrokerChannelListenerBinding(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerQueue queue,
        int prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        bool isEphemeral,
        MessageBrokerChannelListenerBindingState state,
        MessageBrokerQueueListenerBindingState queueBindingState)
    {
        Client = client;
        Channel = channel;
        _state = state;
        _isEphemeral = isEphemeral;

        SetProperties(
            prefetchHint,
            maxRetries,
            retryDelay,
            maxRedeliveries,
            minAckTimeout,
            deadLetterCapacityHint,
            minDeadLetterRetention,
            filterExpression );

        QueueBindingCollection = QueueBindingCollection.Create(
            new MessageBrokerQueueListenerBinding(
                client,
                this,
                queue,
                prefetchHint,
                maxRetries,
                retryDelay,
                maxRedeliveries,
                minAckTimeout,
                deadLetterCapacityHint,
                minDeadLetterRetention,
                filterExpression,
                filterExpressionDelegate?.Delegate,
                queueBindingState,
                isPrimary: true ) );
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this listener belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> instance to which the <see cref="Client"/> is bound to as a listener.
    /// </summary>
    public MessageBrokerChannel Channel { get; }

    /// <summary>
    /// <see cref="MessageBrokerQueue"/> instance to which messages intended for this listener get enqueued into.
    /// </summary>
    public MessageBrokerQueue Queue => QueueBindingCollection.Primary.Value.Queue;

    /// <summary>
    /// Specifies how many messages intended for this listener can be sent by the <see cref="Queue"/>
    /// to the <see cref="Client"/> at the same time.
    /// </summary>
    public int PrefetchHint
    {
        get
        {
            using ( AcquireLock() )
                return _prefetchHint;
        }
    }

    /// <summary>
    /// Specifies how many times the <see cref="Queue"/> will attempt to automatically send a message notification retry
    /// when the <see cref="Client"/> responds with a negative ACK, before giving up.
    /// </summary>
    public int MaxRetries
    {
        get
        {
            using ( AcquireLock() )
                return _maxRetries;
        }
    }

    /// <summary>
    /// Specifies the delay between the <see cref="Queue"/> successfully processing negative ACK sent by the <see cref="Client"/>
    /// and the <see cref="Queue"/> sending a message notification retry.
    /// </summary>
    public Duration RetryDelay
    {
        get
        {
            using ( AcquireLock() )
                return _retryDelay;
        }
    }

    /// <summary>
    /// Specifies how many times the <see cref="Queue"/> will attempt to automatically send a message notification redelivery
    /// when the <see cref="Client"/> fails to respond with either an ACK or a negative ACK in time (see <see cref="MinAckTimeout"/>),
    /// before giving up.
    /// </summary>
    public int MaxRedeliveries
    {
        get
        {
            using ( AcquireLock() )
                return _maxRedeliveries;
        }
    }

    /// <summary>
    /// Specifies the minimum amount of time that the <see cref="Queue"/> will wait for the <see cref="Client"/>
    /// to send either an ACK or a negative ACK before attempting a message notification redelivery.
    /// Actual ACK timeout may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public Duration MinAckTimeout
    {
        get
        {
            using ( AcquireLock() )
                return _minAckTimeout;
        }
    }

    /// <summary>
    /// Specifies how many messages intended for this listener can be stored at most by the <see cref="Queue"/>'s dead letter.
    /// Actual capacity may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public int DeadLetterCapacityHint
    {
        get
        {
            using ( AcquireLock() )
                return _deadLetterCapacityHint;
        }
    }

    /// <summary>
    /// Specifies the minimum retention period for messages intended for this listener stored in the <see cref="Queue"/>'s dead letter.
    /// Actual retention period may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public Duration MinDeadLetterRetention
    {
        get
        {
            using ( AcquireLock() )
                return _minDeadLetterRetention;
        }
    }

    /// <summary>
    /// Specifies message filter expression.
    /// </summary>
    public string? FilterExpression
    {
        get
        {
            using ( AcquireLock() )
                return _filterExpression;
        }
    }

    /// <summary>
    /// Specifies whether the listener is ephemeral.
    /// </summary>
    public bool IsEphemeral
    {
        get
        {
            using ( AcquireLock() )
                return _isEphemeral;
        }
    }

    /// <summary>
    /// Specifies whether the <see cref="Client"/> is expected to send ACK or negative ACK to the <see cref="Queue"/>
    /// in order to confirm message notification.
    /// </summary>
    public bool AreAcksEnabled => MinAckTimeout > Duration.Zero;

    /// <summary>
    /// Current listener's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelListenerBindingState"/> for more information.</remarks>
    public MessageBrokerChannelListenerBindingState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerQueueListenerBinding"/> instances attached to this channel listener, identified by queue ids.
    /// </summary>
    public MessageBrokerChannelListenerBindingQueueBindingCollection QueueBindings =>
        new MessageBrokerChannelListenerBindingQueueBindingCollection( this );

    internal bool IsInactive => _state >= MessageBrokerChannelListenerBindingState.Deactivating;
    internal bool IsDisposed => _state >= MessageBrokerChannelListenerBindingState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelListenerBinding"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var queue = Queue;
        return
            $"[{Client.Id}] '{Client.Name}' => [{Channel.Id}] '{Channel.Name}' listener binding (using [{queue.Id}] '{queue.Name}' queue) ({State})";
    }

    /// <summary>
    /// Deletes this listener from the server.
    /// </summary>
    /// <exception cref="MessageBrokerServerException">
    /// When server is in <see cref="MessageBrokerServerState.Created"/> or <see cref="MessageBrokerServerState.Starting"/> state.
    /// </exception>
    /// <exception cref="MessageBrokerRemoteClientException">
    /// When this listener is in <see cref="MessageBrokerChannelListenerBindingState.Created"/> state.
    /// </exception>
    /// <returns>A task that represents the asynchronous delete operation.</returns>
    public async ValueTask DeleteAsync()
    {
        if ( Client.Server.State < MessageBrokerServerState.Running )
            ExceptionThrower.Throw( Client.Server.Exception( Resources.ServerIsNotRunning ) );

        TaskCompletionSource? deactivated;
        while ( true )
        {
            MessageBrokerChannelListenerBindingState state;

            using ( AcquireLock() )
            {
                state = _state;
                if ( state == MessageBrokerChannelListenerBindingState.Created )
                    ExceptionThrower.Throw( Client.Exception( Resources.ListenerIsBeingBound ) );

                if ( ! IsInactive )
                {
                    _state = MessageBrokerChannelListenerBindingState.Disposing;
                    QueueBindingCollection.DisposeAll();
                    _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                    deactivated = _deactivated;
                    break;
                }

                deactivated = _deactivated;
            }

            if ( state is MessageBrokerChannelListenerBindingState.Deactivating or MessageBrokerChannelListenerBindingState.Disposing
                && deactivated is not null )
                await deactivated.Task.ConfigureAwait( false );

            if ( state >= MessageBrokerChannelListenerBindingState.Disposing )
                return;

            using ( AcquireLock() )
            {
                state = _state;
                if ( state == MessageBrokerChannelListenerBindingState.Disposed )
                    return;

                if ( state == MessageBrokerChannelListenerBindingState.Inactive )
                {
                    _state = MessageBrokerChannelListenerBindingState.Disposing;
                    QueueBindingCollection.DisposeAll();
                    _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
                    deactivated = _deactivated;
                    break;
                }
            }
        }

        try
        {
            ulong? traceId = null;
            ServerStorage.Client clientStorage = default;

            using ( Client.AcquireLock() )
            {
                if ( Client.IsDisposed )
                    clientStorage = Client.GetStorage();
                else
                    traceId = Client.GetTraceId();
            }

            if ( traceId is null )
            {
                await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false );
                return;
            }

            await DeleteAsyncCore( traceId.Value ).ConfigureAwait( false );
        }
        finally
        {
            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelListenerBindingState.Disposed;
                _deactivated = null;
                QueueBindingCollection.Clear();
            }

            deactivated.TrySetResult();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelListenerBinding Create(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerQueue queue,
        in Protocol.BindListenerRequestHeader header,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        bool isEphemeral)
    {
        return new MessageBrokerChannelListenerBinding(
            client,
            channel,
            queue,
            header.PrefetchHint,
            header.MaxRetries,
            header.RetryDelay,
            header.MaxRedeliveries,
            header.MinAckTimeout,
            header.DeadLetterCapacityHint,
            header.MinDeadLetterRetention,
            filterExpression,
            filterExpressionDelegate,
            isEphemeral,
            MessageBrokerChannelListenerBindingState.Created,
            MessageBrokerQueueListenerBindingState.Created );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerChannelListenerBinding CreateInactive(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerQueue queue,
        in Storage.ListenerMetadata metadata,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate)
    {
        return new MessageBrokerChannelListenerBinding(
            client,
            channel,
            queue,
            metadata.PrefetchHint,
            metadata.MaxRetries,
            metadata.RetryDelay,
            metadata.MaxRedeliveries,
            metadata.MinAckTimeout,
            metadata.DeadLetterCapacityHint,
            metadata.MinDeadLetterRetention,
            filterExpression,
            filterExpressionDelegate,
            isEphemeral: false,
            MessageBrokerChannelListenerBindingState.Inactive,
            MessageBrokerQueueListenerBindingState.Inactive );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReactivationResult TryReactivate(
        string queueName,
        in Protocol.BindListenerRequestHeader header,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        bool isEphemeral,
        [NotNull] ref MessageBrokerQueueListenerBinding? existingPrimaryBinding,
        [NotNull] ref MessageBrokerQueueListenerBinding? primaryBinding,
        ref MessageBrokerQueueListenerBinding[] secondaryBindings,
        ref bool disposingExistingPrimaryBinding)
    {
        using ( AcquireLock() )
        {
            var primary = QueueBindingCollection.Primary.Value;
            existingPrimaryBinding = primary;
            primaryBinding = primary;

            if ( _state != MessageBrokerChannelListenerBindingState.Inactive )
                return ReactivationResult.AlreadyBound;

            if ( primary.Queue.Name.Equals( queueName, StringComparison.OrdinalIgnoreCase ) )
            {
                if ( primary.TryReactivate( in header, filterExpression, filterExpressionDelegate, isPrimary: true ) != true )
                {
                    disposingExistingPrimaryBinding = true;
                    return ReactivationResult.AlreadyBound;
                }

                _isEphemeral = isEphemeral;
                _state = MessageBrokerChannelListenerBindingState.Created;

                SetProperties(
                    header.PrefetchHint,
                    header.MaxRetries,
                    header.RetryDelay,
                    header.MaxRedeliveries,
                    header.MinAckTimeout,
                    header.DeadLetterCapacityHint,
                    header.MinDeadLetterRetention,
                    filterExpression );

                if ( QueueBindingCollection.Secondary.Length > 0 )
                {
                    var i = 0;
                    secondaryBindings = new MessageBrokerQueueListenerBinding[QueueBindingCollection.Secondary.Length];
                    foreach ( var binding in QueueBindingCollection.Secondary )
                    {
                        if ( binding.TryReactivate( in header, filterExpression, filterExpressionDelegate, isPrimary: false ) == true )
                            secondaryBindings[i++] = binding;
                    }

                    if ( i < secondaryBindings.Length )
                        Array.Resize( ref secondaryBindings, i );
                }

                return ReactivationResult.Reactivated;
            }

            for ( var i = 0; i < QueueBindingCollection.Secondary.Length; ++i )
            {
                var binding = QueueBindingCollection.Secondary[i];
                if ( ! binding.Queue.Name.Equals( queueName, StringComparison.OrdinalIgnoreCase ) )
                    continue;

                primaryBinding = binding;
                if ( binding.TryReactivate( in header, filterExpression, filterExpressionDelegate, isPrimary: true ) != true )
                {
                    disposingExistingPrimaryBinding = true;
                    return ReactivationResult.AlreadyBound;
                }

                _isEphemeral = isEphemeral;
                _state = MessageBrokerChannelListenerBindingState.Created;
                QueueBindingCollection.Primary.Write( binding );
                QueueBindingCollection.Secondary[i] = primary;

                var j = 0;
                secondaryBindings = new MessageBrokerQueueListenerBinding[QueueBindingCollection.Secondary.Length];
                foreach ( var b in QueueBindingCollection.Secondary )
                {
                    var reactivated = b.TryReactivate(
                        in header,
                        filterExpression,
                        filterExpressionDelegate,
                        isPrimary: false,
                        disposeIfUnreferenced: ReferenceEquals( b, primary ) );

                    if ( reactivated is null )
                        disposingExistingPrimaryBinding = true;
                    else if ( reactivated == true )
                        secondaryBindings[j++] = b;
                }

                if ( j < secondaryBindings.Length )
                    Array.Resize( ref secondaryBindings, j );

                return ReactivationResult.Reactivated;
            }

            _state = MessageBrokerChannelListenerBindingState.Created;
        }

        return ReactivationResult.Rebinding;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryReactivateWithNewPrimaryQueue(
        MessageBrokerQueue queue,
        in Protocol.BindListenerRequestHeader header,
        string? filterExpression,
        IParsedExpressionDelegate<MessageBrokerFilterExpressionContext, bool>? filterExpressionDelegate,
        bool isEphemeral,
        [NotNull] ref MessageBrokerQueueListenerBinding? existingPrimaryBinding,
        [NotNull] ref MessageBrokerQueueListenerBinding? primaryBinding,
        ref MessageBrokerQueueListenerBinding[] secondaryBindings,
        ref bool disposingExistingPrimaryBinding)
    {
        using ( AcquireLock() )
        {
            var primary = QueueBindingCollection.Primary.Value;
            existingPrimaryBinding = primary;
            primaryBinding = new MessageBrokerQueueListenerBinding(
                Client,
                this,
                queue,
                header.PrefetchHint,
                header.MaxRetries,
                header.RetryDelay,
                header.MaxRedeliveries,
                header.MinAckTimeout,
                header.DeadLetterCapacityHint,
                header.MinDeadLetterRetention,
                filterExpression,
                filterExpressionDelegate?.Delegate,
                MessageBrokerQueueListenerBindingState.Created,
                isPrimary: true );

            if ( _state != MessageBrokerChannelListenerBindingState.Created )
            {
                primaryBinding.TryMarkAsDisposed();
                return false;
            }

            _isEphemeral = isEphemeral;
            QueueBindingCollection.Primary.Write( primaryBinding );
            QueueBindingCollection.AddSecondaryUnsafe( primary );

            var i = 0;
            secondaryBindings = new MessageBrokerQueueListenerBinding[QueueBindingCollection.Secondary.Length];
            foreach ( var b in QueueBindingCollection.Secondary )
            {
                var reactivated = b.TryReactivate(
                    in header,
                    filterExpression,
                    filterExpressionDelegate,
                    isPrimary: false,
                    disposeIfUnreferenced: ReferenceEquals( b, primary ) );

                if ( reactivated is null )
                    disposingExistingPrimaryBinding = true;
                else if ( reactivated == true )
                    secondaryBindings[i++] = b;
            }

            if ( i < secondaryBindings.Length )
                Array.Resize( ref secondaryBindings, i );
        }

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal MessageBrokerQueueListenerBinding? AddSecondaryQueueBinding(MessageBrokerQueue queue)
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return null;

            var result = QueueBindingCollection.Primary.Value.CloneInactive( queue );
            QueueBindingCollection.AddSecondaryUnsafe( result );
            return result;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RemoveSecondaryQueueBinding(MessageBrokerQueueListenerBinding binding)
    {
        using ( AcquireLock() )
        {
            if ( ! IsDisposed )
                QueueBindingCollection.RemoveSecondaryUnsafe( binding );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void RevertRebinding()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelListenerBindingState.Created )
                _state = MessageBrokerChannelListenerBindingState.Inactive;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsEphemeral()
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelListenerBindingState.Inactive )
                _isEphemeral = true;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsRunning()
    {
        using ( AcquireLock() )
        {
            if ( _state != MessageBrokerChannelListenerBindingState.Created )
                return;

            _state = MessageBrokerChannelListenerBindingState.Running;
            QueueBindingCollection.Primary.Value.MarkAsRunning();
            foreach ( var b in QueueBindingCollection.Secondary )
                b.MarkAsRunning();
        }
    }

    internal bool OnServerDisposing()
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return false;

            _state = MessageBrokerChannelListenerBindingState.Disposing;
            QueueBindingCollection.DisposeAll();
            _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            _autoDisposed = true;
        }

        return true;
    }

    internal void OnClientDeactivating(bool keepAlive)
    {
        using ( AcquireLock() )
        {
            if ( _state == MessageBrokerChannelListenerBindingState.Disposed )
                return;

            if ( keepAlive && ! _isEphemeral )
            {
                if ( IsInactive )
                    return;

                _state = MessageBrokerChannelListenerBindingState.Deactivating;
                QueueBindingCollection.DeactivateAll();
            }
            else
            {
                _state = MessageBrokerChannelListenerBindingState.Disposing;
                QueueBindingCollection.DisposeAll();
            }

            _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            _autoDisposed = true;
        }
    }

    internal async ValueTask OnServerDisposedAsync(
        ServerStorage.Client clientStorage,
        bool clearBuffers,
        bool storageLoaded,
        ulong clientTraceId)
    {
        bool isEphemeral;
        bool autoDisposed;
        MessageBrokerChannelListenerBindingState state;
        TaskCompletionSource? deactivated;

        using ( AcquireLock() )
        {
            state = _state;
            Assume.IsGreaterThanOrEqualTo( state, MessageBrokerChannelListenerBindingState.Disposing );
            autoDisposed = _autoDisposed;
            deactivated = _deactivated;
            isEphemeral = _isEphemeral;
        }

        if ( ! autoDisposed )
        {
            if ( state == MessageBrokerChannelListenerBindingState.Disposing )
            {
                if ( storageLoaded )
                    Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

                Client.EmitError( await (deactivated?.Task).AsSafeCancellable().ConfigureAwait( false ), clientTraceId );
            }

            return;
        }

        try
        {
            if ( isEphemeral )
            {
                QueueBindingCollection.RemoveAllFromQueues( Channel.Id );
                using ( Channel.AcquireLock() )
                    Channel.ListenersByClientId.Remove( Client.Id );
            }
            else
                Client.EmitError(
                    await clientStorage.SaveMetadataAsync( this, clearBuffers, clientTraceId ).AsSafe().ConfigureAwait( false ),
                    clientTraceId );

            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelListenerBindingState.Disposed;
                _autoDisposed = false;
                _deactivated = null;
                QueueBindingCollection.Clear();
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    internal async ValueTask OnClientDeactivatedAsync(ServerStorage.Client clientStorage, bool keepAlive, ulong clientTraceId)
    {
        bool dispose;
        bool autoDisposed;
        MessageBrokerChannelListenerBindingState state;
        TaskCompletionSource? deactivated;

        using ( AcquireLock() )
        {
            state = _state;
            Assume.IsGreaterThanOrEqualTo( state, MessageBrokerChannelListenerBindingState.Deactivating );
            if ( state == MessageBrokerChannelListenerBindingState.Inactive && keepAlive )
                return;

            autoDisposed = _autoDisposed;
            deactivated = _deactivated;
            dispose = _isEphemeral || ! keepAlive;
        }

        if ( ! autoDisposed )
        {
            if ( state == MessageBrokerChannelListenerBindingState.Disposing )
            {
                Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );
                Client.EmitError( await (deactivated?.Task).AsSafeCancellable().ConfigureAwait( false ), clientTraceId );
            }

            return;
        }

        try
        {
            if ( dispose )
            {
                if ( keepAlive )
                {
                    using ( Client.AcquireLock() )
                        Client.ListenersByChannelId.Remove( Channel.Id );

                    QueueBindingCollection.RemoveAllFromQueues( Channel.Id );
                }

                await Channel.OnListenerDisposingAsync( Client, clientTraceId ).ConfigureAwait( false );
                Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

                using ( AcquireLock() )
                {
                    _state = MessageBrokerChannelListenerBindingState.Disposed;
                    _autoDisposed = false;
                    _deactivated = null;
                    QueueBindingCollection.Clear();
                }
            }
            else
            {
                using ( AcquireLock() )
                {
                    _state = MessageBrokerChannelListenerBindingState.Inactive;
                    _autoDisposed = false;
                    _deactivated = null;
                }
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryBeginDisposingUnsafe(
        [MaybeNullWhen( false )] out MessageBrokerQueueListenerBinding primaryBinding,
        out ReadOnlyArray<MessageBrokerQueueListenerBinding> secondaryBindings)
    {
        if ( IsInactive )
        {
            if ( _state != MessageBrokerChannelListenerBindingState.Inactive )
            {
                primaryBinding = null;
                secondaryBindings = ReadOnlyArray<MessageBrokerQueueListenerBinding>.Empty;
                return false;
            }
        }

        _state = MessageBrokerChannelListenerBindingState.Disposing;
        primaryBinding = QueueBindingCollection.Primary.Value;
        secondaryBindings = QueueBindingCollection.Secondary;
        _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );

        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal async ValueTask EndDisposingAsync(ServerStorage.Client clientStorage, ulong clientTraceId)
    {
        TaskCompletionSource? deactivated = null;
        try
        {
            Assume.Equals( State, MessageBrokerChannelListenerBindingState.Disposing );
            Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

            using ( Client.AcquireLock() )
            {
                if ( ! Client.IsDisposed )
                    Client.ListenersByChannelId.Remove( Channel.Id );
            }

            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelListenerBindingState.Disposed;
                deactivated = _deactivated;
                _deactivated = null;
                QueueBindingCollection.Clear();
            }
        }
        finally
        {
            deactivated?.TrySetResult();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetProperties(
        int prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        string? filterExpression)
    {
        Assume.IsGreaterThan( prefetchHint, 0 );
        Assume.IsGreaterThanOrEqualTo( maxRetries, 0 );
        Assume.IsGreaterThanOrEqualTo( retryDelay, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( maxRedeliveries, 0 );
        Assume.IsGreaterThanOrEqualTo( minAckTimeout, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( deadLetterCapacityHint, 0 );
        Assume.IsGreaterThanOrEqualTo( minDeadLetterRetention, Duration.Zero );

        _prefetchHint = prefetchHint;
        _maxRetries = maxRetries;
        _retryDelay = retryDelay;
        _maxRedeliveries = maxRedeliveries;
        _minAckTimeout = minAckTimeout;
        _deadLetterCapacityHint = deadLetterCapacityHint;
        _minDeadLetterRetention = minDeadLetterRetention;
        _filterExpression = filterExpression;
    }

    private async ValueTask DeleteAsyncCore(ulong clientTraceId)
    {
        var notificationEnqueued = false;
        var notificationPoolToken = MemoryPoolToken<byte>.Empty;
        if ( Client.Logger.TraceStart is { } traceStart )
            traceStart.Emit(
                MessageBrokerRemoteClientTraceEvent.Create(
                    Client,
                    clientTraceId,
                    MessageBrokerRemoteClientTraceEventType.UnbindListener ) );

        try
        {
            var disposingChannel = false;
            var channelTraceId = 0UL;
            QueueListenerUnbindingPayload primaryPayload = default;
            var secondaryPayloads = Array.Empty<QueueListenerUnbindingPayload>();
            var clearBuffers = true;
            ServerStorage.Client clientStorage;
            Exception? exception = null;

            using ( Client.AcquireLock() )
            {
                clientStorage = Client.GetStorage();
                if ( Client.IsDisposed )
                    exception = Client.DeactivatedException( disposed: true );
                else
                {
                    clearBuffers = Client.GetClearBuffersOption();

                    using ( Channel.AcquireLock() )
                    {
                        if ( Channel.IsDisposed )
                            exception = Channel.DisposedException();
                        else
                        {
                            MessageBrokerQueueListenerBinding? primary;
                            ReadOnlyArray<MessageBrokerQueueListenerBinding> secondary;
                            using ( AcquireLock() )
                            {
                                Assume.Equals( _state, MessageBrokerChannelListenerBindingState.Disposing );
                                primary = QueueBindingCollection.Primary.Value;
                                secondary = QueueBindingCollection.Secondary;

                                disposingChannel = Channel.TryDisposeByRemovingListenerUnsafe( Client.Id );
                                channelTraceId = Channel.GetTraceId();
                            }

                            using ( primary.Queue.AcquireLock() )
                            {
                                if ( primary.Queue.IsDisposed )
                                    exception = primary.Queue.DisposedException();
                                else
                                {
                                    var disposingQueue = primary.Queue.TryDisposeByRemovingListenerUnsafe( Channel.Id );
                                    var queueTraceId = primary.Queue.GetTraceId();
                                    primaryPayload = new QueueListenerUnbindingPayload( primary, queueTraceId, disposingQueue );
                                }
                            }

                            if ( secondary.Count > 0 )
                            {
                                var i = 0;
                                secondaryPayloads = new QueueListenerUnbindingPayload[secondary.Count];
                                foreach ( var binding in secondary )
                                {
                                    using ( binding.Queue.AcquireLock() )
                                    {
                                        if ( ! binding.Queue.IsDisposed )
                                        {
                                            var disposingQueue = binding.Queue.TryDisposeByRemovingListenerUnsafe( Channel.Id );
                                            var queueTraceId = binding.Queue.GetTraceId();
                                            secondaryPayloads[i++] = new QueueListenerUnbindingPayload(
                                                binding,
                                                queueTraceId,
                                                disposingQueue );
                                        }
                                    }
                                }

                                if ( i < secondaryPayloads.Length )
                                    Array.Resize( ref secondaryPayloads, i );
                            }
                        }
                    }
                }
            }

            Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );
            if ( exception is not null )
            {
                if ( Client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( Client, clientTraceId, exception ) );

                return;
            }

            var disposingPrimaryQueue = false;
            if ( primaryPayload.Binding is not null )
            {
                var queue = primaryPayload.Binding.Queue;
                disposingPrimaryQueue = primaryPayload.DisposingQueue;
                using ( MessageBrokerQueueTraceEvent.CreateScope(
                    queue,
                    primaryPayload.QueueTraceId,
                    MessageBrokerQueueTraceEventType.UnbindListener ) )
                {
                    if ( queue.Logger.ClientTrace is { } clientTrace )
                        clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, primaryPayload.QueueTraceId, clientTraceId ) );

                    if ( queue.Logger.ListenerUnbound is { } queueListenerUnbound )
                        queueListenerUnbound.Emit(
                            MessageBrokerQueueListenerUnboundEvent.Create(
                                primaryPayload.Binding,
                                primaryPayload.QueueTraceId,
                                disposingChannel ) );

                    if ( primaryPayload.DisposingQueue )
                        await queue.DisposeDueToLackOfReferencesAsync( primaryPayload.QueueTraceId ).ConfigureAwait( false );
                }
            }

            foreach ( var payload in secondaryPayloads )
            {
                Assume.IsNotNull( payload.Binding );
                var queue = payload.Binding.Queue;
                using ( MessageBrokerQueueTraceEvent.CreateScope(
                    queue,
                    payload.QueueTraceId,
                    MessageBrokerQueueTraceEventType.UnbindListener ) )
                {
                    if ( queue.Logger.ClientTrace is { } clientTrace )
                        clientTrace.Emit( MessageBrokerQueueClientTraceEvent.Create( queue, payload.QueueTraceId, clientTraceId ) );

                    if ( queue.Logger.ListenerUnbound is { } queueListenerUnbound )
                        queueListenerUnbound.Emit(
                            MessageBrokerQueueListenerUnboundEvent.Create( payload.Binding, payload.QueueTraceId, disposingChannel ) );

                    if ( payload.DisposingQueue )
                        await queue.DisposeDueToLackOfReferencesAsync( payload.QueueTraceId ).ConfigureAwait( false );
                }
            }

            using ( MessageBrokerChannelTraceEvent.CreateScope(
                Channel,
                channelTraceId,
                MessageBrokerChannelTraceEventType.UnbindListener ) )
            {
                if ( Channel.Logger.ClientTrace is { } clientTrace )
                    clientTrace.Emit( MessageBrokerChannelClientTraceEvent.Create( Channel, channelTraceId, Client, clientTraceId ) );

                if ( Channel.Logger.ListenerUnbound is { } channelListenerUnbound )
                    channelListenerUnbound.Emit(
                        MessageBrokerChannelListenerUnboundEvent.Create( this, channelTraceId, disposingPrimaryQueue ) );

                if ( disposingChannel )
                    await Channel.DisposeDueToLackOfReferencesAsync( channelTraceId ).ConfigureAwait( false );
            }

            using ( Client.AcquireLock() )
            {
                if ( ! Client.IsDisposed )
                    Client.ListenersByChannelId.Remove( Channel.Id );
            }

            using ( AcquireLock() )
            {
                _state = MessageBrokerChannelListenerBindingState.Disposed;
                QueueBindingCollection.Clear();
            }

            if ( Client.Logger.ListenerUnbound is { } listenerUnbound )
                listenerUnbound.Emit(
                    MessageBrokerRemoteClientListenerUnboundEvent.Create( this, clientTraceId, disposingChannel, disposingPrimaryQueue ) );

            var notification = new Protocol.ChannelBindingDeletedNotification(
                MessageBrokerSystemNotificationType.ListenerDeleted,
                Channel.Name );

            var notificationLength = notification.Length;
            notificationPoolToken = Client.MemoryPool.Rent( notificationLength, clearBuffers, out var responseData );
            notification.Serialize( responseData );

            using ( Client.AcquireLock() )
            {
                if ( Client.IsInactive )
                    exception = Client.IsDisposed ? Client.DeactivatedException( disposed: true ) : null;
                else
                {
                    var writerSource = Client.WriterQueue.AcquireSource( responseData, clearBuffers );
                    ResponseSender.EnqueueUnsafe(
                        Client,
                        notification.Header,
                        writerSource,
                        notificationPoolToken,
                        MessageBrokerRemoteClientTraceEventType.UnbindListener,
                        clientTraceId );

                    notificationEnqueued = true;
                    Client.ResponseSender.SignalContinuation();
                }
            }

            if ( exception is not null )
            {
                if ( Client.Logger.Error is { } error )
                    error.Emit( MessageBrokerRemoteClientErrorEvent.Create( Client, clientTraceId, exception ) );
            }
        }
        finally
        {
            if ( ! notificationEnqueued )
            {
                notificationPoolToken.Return( Client, clientTraceId );
                if ( Client.Logger.TraceEnd is { } traceEnd )
                    traceEnd.Emit(
                        MessageBrokerRemoteClientTraceEvent.Create(
                            Client,
                            clientTraceId,
                            MessageBrokerRemoteClientTraceEventType.UnbindListener ) );
            }
        }
    }
}
