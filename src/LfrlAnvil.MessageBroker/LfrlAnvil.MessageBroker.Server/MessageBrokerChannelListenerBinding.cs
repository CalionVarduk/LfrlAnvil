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
using LfrlAnvil.Computable.Expressions;
using LfrlAnvil.Extensions;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel binding for a listener, which allows clients to listen to messages published through channels.
/// </summary>
public sealed class MessageBrokerChannelListenerBinding
{
    private const int DisposedCounterValue = int.MinValue;
    private const int InactiveZeroCounterValue = -1;

    private readonly object _sync = new object();
    private readonly MessageBrokerFilterExpressionContext[] _filterPredicateArgs;
    private readonly Func<MessageBrokerFilterExpressionContext[], bool>? _filterPredicate;
    private TaskCompletionSource? _deactivated;
    private InterlockedInt32 _prefetchCounter;
    private InterlockedInt32 _deadLetterCounter;
    private InterlockedBoolean _isEphemeral;
    private InterlockedInt32 _state;
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
        int counter,
        bool isEphemeral,
        MessageBrokerChannelListenerBindingState state)
    {
        Assume.IsGreaterThan( prefetchHint, 0 );
        Assume.IsGreaterThanOrEqualTo( maxRetries, 0 );
        Assume.IsGreaterThanOrEqualTo( retryDelay, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( maxRedeliveries, 0 );
        Assume.IsGreaterThanOrEqualTo( minAckTimeout, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( deadLetterCapacityHint, 0 );
        Assume.IsGreaterThanOrEqualTo( minDeadLetterRetention, Duration.Zero );
        Client = client;
        Channel = channel;
        Queue = queue;
        _state = new InterlockedInt32( ( int )state );
        PrefetchHint = prefetchHint;
        MaxRetries = maxRetries;
        RetryDelay = retryDelay;
        MaxRedeliveries = maxRedeliveries;
        MinAckTimeout = minAckTimeout;
        DeadLetterCapacityHint = deadLetterCapacityHint;
        MinDeadLetterRetention = minDeadLetterRetention;
        _filterPredicateArgs = filterExpression is not null ? [ default ] : Array.Empty<MessageBrokerFilterExpressionContext>();
        _filterPredicate = filterExpressionDelegate?.Delegate;
        FilterExpression = filterExpression;
        _prefetchCounter = new InterlockedInt32( counter );
        _deadLetterCounter = new InterlockedInt32( counter );
        _isEphemeral = new InterlockedBoolean( isEphemeral );
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
    public MessageBrokerQueue Queue { get; }

    /// <summary>
    /// Specifies how many messages intended for this listener can be sent by the <see cref="Queue"/>
    /// to the <see cref="Client"/> at the same time.
    /// </summary>
    public int PrefetchHint { get; }

    /// <summary>
    /// Specifies how many times the <see cref="Queue"/> will attempt to automatically send a message notification retry
    /// when the <see cref="Client"/> responds with a negative ACK, before giving up.
    /// </summary>
    public int MaxRetries { get; }

    /// <summary>
    /// Specifies the delay between the <see cref="Queue"/> successfully processing negative ACK sent by the <see cref="Client"/>
    /// and the <see cref="Queue"/> sending a message notification retry.
    /// </summary>
    public Duration RetryDelay { get; }

    /// <summary>
    /// Specifies how many times the <see cref="Queue"/> will attempt to automatically send a message notification redelivery
    /// when the <see cref="Client"/> fails to respond with either an ACK or a negative ACK in time (see <see cref="MinAckTimeout"/>),
    /// before giving up.
    /// </summary>
    public int MaxRedeliveries { get; }

    /// <summary>
    /// Specifies the minimum amount of time that the <see cref="Queue"/> will wait for the <see cref="Client"/>
    /// to send either an ACK or a negative ACK before attempting a message notification redelivery.
    /// Actual ACK timeout may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public Duration MinAckTimeout { get; }

    /// <summary>
    /// Specifies how many messages intended for this listener can be stored at most by the <see cref="Queue"/>'s dead letter.
    /// Actual capacity may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public int DeadLetterCapacityHint { get; }

    /// <summary>
    /// Specifies the minimum retention period for messages intended for this listener stored in the <see cref="Queue"/>'s dead letter.
    /// Actual retention period may be different due to the state of the <see cref="Queue"/> and other listeners bound to it.
    /// </summary>
    public Duration MinDeadLetterRetention { get; }

    /// <summary>
    /// Specifies message filter expression.
    /// </summary>
    public string? FilterExpression { get; }

    /// <summary>
    /// Specifies whether the listener is ephemeral.
    /// </summary>
    public bool IsEphemeral => _isEphemeral.Value;

    /// <summary>
    /// Specifies whether the <see cref="Client"/> is expected to send ACK or negative ACK to the <see cref="Queue"/>
    /// in order to confirm message notification.
    /// </summary>
    public bool AreAcksEnabled => MinAckTimeout > Duration.Zero;

    /// <summary>
    /// Current listener's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelListenerBindingState"/> for more information.</remarks>
    public MessageBrokerChannelListenerBindingState State => ( MessageBrokerChannelListenerBindingState )_state.Value;

    internal bool IsInactive => State >= MessageBrokerChannelListenerBindingState.Deactivating;
    internal bool IsDisposed => State >= MessageBrokerChannelListenerBindingState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelListenerBinding"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[{Client.Id}] '{Client.Name}' => [{Channel.Id}] '{Channel.Name}' listener binding (using [{Queue.Id}] '{Queue.Name}' queue) ({State})";
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
            counter: 0,
            isEphemeral,
            MessageBrokerChannelListenerBindingState.Running );
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
            counter: InactiveZeroCounterValue,
            isEphemeral: false,
            MessageBrokerChannelListenerBindingState.Inactive );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void MarkAsEphemeral()
    {
        using ( AcquireLock() )
        {
            var state = ( MessageBrokerChannelListenerBindingState )_state.Value;
            if ( state == MessageBrokerChannelListenerBindingState.Inactive )
                _isEphemeral.WriteTrue();
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool FilterMessage(in StreamMessage message, ulong queueTraceId)
    {
        Assume.IsNotNull( _filterPredicate );
        var context = new MessageBrokerFilterExpressionContext( this, in message );
        MessageBrokerFilterExpressionContext[] args;
        using ( AcquireLock() )
        {
            Assume.Equals( _filterPredicateArgs[0], default );
            _filterPredicateArgs[0] = context;
            args = _filterPredicateArgs;
        }

        try
        {
            return _filterPredicate( args );
        }
        catch ( Exception exc )
        {
            if ( Queue.Logger.Error is { } error )
                error.Emit( MessageBrokerQueueErrorEvent.Create( Queue, queueTraceId, exc ) );

            return true;
        }
        finally
        {
            using ( AcquireLock() )
                _filterPredicateArgs[0] = default;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryIncrementPrefetchCounter(out bool disposed)
    {
        int current;
        do
        {
            current = _prefetchCounter.Value;
            if ( current <= InactiveZeroCounterValue )
            {
                disposed = current == DisposedCounterValue;
                return false;
            }

            if ( current >= PrefetchHint )
            {
                disposed = false;
                return false;
            }
        }
        while ( ! _prefetchCounter.Write( unchecked( current + 1 ), current ) );

        disposed = false;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void IncrementInactivePrefetchCounter()
    {
        int current;
        do
        {
            current = _prefetchCounter.Value;
            Assume.IsLessThan( current, 0 );
            if ( current == DisposedCounterValue )
                return;

            Ensure.IsGreaterThan( current, DisposedCounterValue + 1 );
        }
        while ( ! _prefetchCounter.Write( unchecked( current - 1 ), current ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool DecrementPrefetchCounter()
    {
        int current;
        do
        {
            current = _prefetchCounter.Value;
            if ( current <= 0 )
                return true;
        }
        while ( ! _prefetchCounter.Write( unchecked( current - 1 ), current ) );

        return current >= PrefetchHint;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanConsumeRetry()
    {
        var current = _prefetchCounter.Value;
        return (current >= 0 && current < PrefetchHint) || current == DisposedCounterValue;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanConsumeUnackedOrDeadLetter()
    {
        var current = _prefetchCounter.Value;
        return current >= 0 || current == DisposedCounterValue;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanSendRedelivery(out bool disposed)
    {
        var current = _prefetchCounter.Value;
        if ( current >= 0 )
        {
            disposed = false;
            return true;
        }

        disposed = current == DisposedCounterValue;
        return false;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool IncrementDeadLetterCounter()
    {
        int current;
        do
        {
            current = _deadLetterCounter.Value;
            if ( current < 0 )
                return IsDisposed;
        }
        while ( ! _deadLetterCounter.Write( checked( current + 1 ), current ) );

        return current == DeadLetterCapacityHint;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void IncrementInactiveDeadLetterCounter()
    {
        int current;
        do
        {
            current = _deadLetterCounter.Value;
            Assume.IsLessThan( current, 0 );
            if ( IsDisposed )
                return;
        }
        while ( ! _deadLetterCounter.Write( checked( current - 1 ), current ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool DecrementDeadLetterCounter(out bool disposed)
    {
        int current;
        do
        {
            current = _deadLetterCounter.Value;
            if ( current == 0 )
                break;

            if ( current < 0 )
            {
                disposed = IsDisposed;
                return false;
            }
        }
        while ( ! _deadLetterCounter.Write( unchecked( current - 1 ), current ) );

        disposed = false;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDecrementDeadLetterCounterIfExceeded(out bool disposed)
    {
        int current;
        do
        {
            current = _deadLetterCounter.Value;
            if ( current < 0 )
            {
                disposed = IsDisposed;
                return false;
            }

            if ( current <= DeadLetterCapacityHint )
            {
                disposed = false;
                return false;
            }
        }
        while ( ! _deadLetterCounter.Write( unchecked( current - 1 ), current ) );

        disposed = false;
        return true;
    }

    internal bool OnServerDisposing()
    {
        using ( AcquireLock() )
        {
            if ( IsDisposed )
                return false;

            _state.Write( ( int )MessageBrokerChannelListenerBindingState.Disposing );
            _prefetchCounter.Write( DisposedCounterValue );
            _deadLetterCounter.Write( DisposedCounterValue );
            _deactivated = new TaskCompletionSource( TaskCreationOptions.RunContinuationsAsynchronously );
            _autoDisposed = true;
        }

        return true;
    }

    internal void OnClientDeactivating(bool keepAlive)
    {
        using ( AcquireLock() )
        {
            if ( State == MessageBrokerChannelListenerBindingState.Disposed )
                return;

            if ( keepAlive && ! IsEphemeral )
            {
                if ( IsInactive )
                    return;

                _state.Write( ( int )MessageBrokerChannelListenerBindingState.Deactivating );
                DeactivateCounter( ref _prefetchCounter );
                DeactivateCounter( ref _deadLetterCounter );
            }
            else
            {
                _state.Write( ( int )MessageBrokerChannelListenerBindingState.Disposing );
                _prefetchCounter.Write( DisposedCounterValue );
                _deadLetterCounter.Write( InactiveZeroCounterValue );
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
            Assume.IsGreaterThanOrEqualTo( State, MessageBrokerChannelListenerBindingState.Disposing );
            state = State;
            autoDisposed = _autoDisposed;
            deactivated = _deactivated;
            isEphemeral = IsEphemeral;
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
                using ( Queue.AcquireLock() )
                    Queue.ListenersByChannelId.Remove( Channel.Id );

                using ( Channel.AcquireLock() )
                    Channel.ListenersByClientId.Remove( Client.Id );
            }
            else
                Client.EmitError(
                    await clientStorage.SaveMetadataAsync( this, clearBuffers, clientTraceId ).AsSafe().ConfigureAwait( false ),
                    clientTraceId );

            using ( AcquireLock() )
            {
                _state.Write( ( int )MessageBrokerChannelListenerBindingState.Disposed );
                _autoDisposed = false;
                _deactivated = null;
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
            state = State;
            Assume.IsGreaterThanOrEqualTo( state, MessageBrokerChannelListenerBindingState.Deactivating );
            if ( state == MessageBrokerChannelListenerBindingState.Inactive && keepAlive )
                return;

            autoDisposed = _autoDisposed;
            deactivated = _deactivated;
            dispose = IsEphemeral || ! keepAlive;
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

                    using ( Queue.AcquireLock() )
                        Queue.ListenersByChannelId.Remove( Channel.Id );
                }

                await Channel.OnListenerDisposingAsync( Client, clientTraceId ).ConfigureAwait( false );
                Client.EmitError( await clientStorage.DeleteAsync( this ).AsSafe().ConfigureAwait( false ), clientTraceId );

                using ( AcquireLock() )
                {
                    _state.Write( ( int )MessageBrokerChannelListenerBindingState.Disposed );
                    _autoDisposed = false;
                    _deactivated = null;
                }
            }
            else
            {
                using ( AcquireLock() )
                {
                    _state.Write( ( int )MessageBrokerChannelListenerBindingState.Inactive );
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
    internal void BeginDisposingUnsafe()
    {
        Assume.Equals( State, MessageBrokerChannelListenerBindingState.Running );
        _state.Write( ( int )MessageBrokerChannelListenerBindingState.Disposing );
        _prefetchCounter.Write( DisposedCounterValue );
        _deadLetterCounter.Write( InactiveZeroCounterValue );
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
                _state.Write( ( int )MessageBrokerChannelListenerBindingState.Disposed );
                deactivated = _deactivated;
                _deactivated = null;
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
    private static void DeactivateCounter(ref InterlockedInt32 counter)
    {
        int current;
        do
        {
            current = counter.Value;
            if ( current <= InactiveZeroCounterValue )
                return;
        }
        while ( ! counter.Write( unchecked( InactiveZeroCounterValue - current ), current ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ActivatePrefetchCounter()
    {
        int current;
        do
        {
            current = _prefetchCounter.Value;
            if ( current >= 0 || current == DisposedCounterValue )
                return;
        }
        while ( ! _prefetchCounter.Write( unchecked( InactiveZeroCounterValue - current ), current ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ActivateDeadLetterCounter()
    {
        int current;
        do
        {
            current = _deadLetterCounter.Value;
            if ( current >= 0 || IsDisposed )
                return;
        }
        while ( ! _deadLetterCounter.Write( unchecked( InactiveZeroCounterValue - current ), current ) );
    }
}
