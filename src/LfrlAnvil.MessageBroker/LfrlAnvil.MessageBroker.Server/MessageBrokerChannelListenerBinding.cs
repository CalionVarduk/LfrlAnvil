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
using LfrlAnvil.Async;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel binding for a listener, which allows clients to listen to messages published through channels.
/// </summary>
public sealed class MessageBrokerChannelListenerBinding
{
    private readonly object _sync = new object();
    private InterlockedInt32 _prefetchCounter;
    private MessageBrokerChannelListenerBindingState _state;

    internal MessageBrokerChannelListenerBinding(
        MessageBrokerRemoteClient client,
        MessageBrokerChannel channel,
        MessageBrokerQueue queue,
        int prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout)
    {
        Assume.IsGreaterThan( prefetchHint, 0 );
        Assume.IsGreaterThanOrEqualTo( maxRetries, 0 );
        Assume.IsGreaterThanOrEqualTo( retryDelay, Duration.Zero );
        Assume.IsGreaterThanOrEqualTo( maxRedeliveries, 0 );
        Assume.IsGreaterThanOrEqualTo( minAckTimeout, Duration.Zero );
        Client = client;
        Channel = channel;
        Queue = queue;
        _state = MessageBrokerChannelListenerBindingState.Running;
        PrefetchHint = prefetchHint;
        MaxRetries = maxRetries;
        RetryDelay = retryDelay;
        MaxRedeliveries = maxRedeliveries;
        MinAckTimeout = minAckTimeout;
        _prefetchCounter = new InterlockedInt32( 0 );
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
    /// Specifies whether or not the <see cref="Client"/> is expected to send ACK or negative ACK to the <see cref="Queue"/>
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

    internal bool ShouldCancel => _state >= MessageBrokerChannelListenerBindingState.Disposing;

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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryIncrementPrefetchCounter(out bool disposed)
    {
        int current;
        do
        {
            current = _prefetchCounter.Value;
            if ( current < 0 )
            {
                disposed = true;
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
    internal bool DecrementPrefetchCounter()
    {
        int current;
        do
        {
            current = _prefetchCounter.Value;
            if ( current < 0 )
                return true;
        }
        while ( ! _prefetchCounter.Write( unchecked( current - 1 ), current ) );

        return current >= PrefetchHint;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanConsumePrefetchCounter()
    {
        return _prefetchCounter.Value < PrefetchHint;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool CanSendRedelivery()
    {
        return _prefetchCounter.Value >= 0;
    }

    internal void OnServerDisposed()
    {
        _prefetchCounter.Write( -1 );
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelListenerBindingState.Disposed;
        }
    }

    internal void OnClientDisconnected(ulong traceId)
    {
        _prefetchCounter.Write( -1 );
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelListenerBindingState.Disposing;
        }

        Channel.OnListenerDisposing( Client, traceId );
        using ( AcquireLock() )
            _state = MessageBrokerChannelListenerBindingState.Disposed;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void BeginDisposingUnsafe()
    {
        Assume.Equals( _state, MessageBrokerChannelListenerBindingState.Running );
        _prefetchCounter.Write( -1 );
        _state = MessageBrokerChannelListenerBindingState.Disposing;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void EndDisposing()
    {
        using ( AcquireLock() )
        {
            Assume.Equals( _state, MessageBrokerChannelListenerBindingState.Disposing );
            _state = MessageBrokerChannelListenerBindingState.Disposed;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }
}
