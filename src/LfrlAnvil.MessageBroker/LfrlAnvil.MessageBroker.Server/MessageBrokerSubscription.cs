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
using LfrlAnvil.MessageBroker.Server.Events;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker subscription, which allows clients to listen to messages published through channels.
/// </summary>
public sealed class MessageBrokerSubscription
{
    private readonly object _sync = new object();
    private readonly MessageBrokerSubscriptionEventHandler? _eventHandler;
    private MessageBrokerSubscriptionState _state;

    internal MessageBrokerSubscription(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        Client = client;
        Channel = channel;
        _state = MessageBrokerSubscriptionState.Running;
        _eventHandler = client.Server.SubscriptionEventHandlerFactory?.Invoke( this );
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this subscription belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> instance to which the <see cref="Client"/> is subscribed to.
    /// </summary>
    public MessageBrokerChannel Channel { get; }

    /// <summary>
    /// Current subscription's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerSubscriptionState"/> for more information.</remarks>
    public MessageBrokerSubscriptionState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    internal bool ShouldCancel => _state >= MessageBrokerSubscriptionState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerSubscription"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Client.Id}] '{Client.Name}' => [{Channel.Id}] '{Channel.Name}' subscription ({State})";
    }

    internal void OnServerDisposed()
    {
        Dispose( notifyChannel: false );
    }

    internal void OnClientDisconnected()
    {
        Dispose( notifyChannel: true );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void BeginDisposingUnsafe()
    {
        Assume.Equals( _state, MessageBrokerSubscriptionState.Running );
        _state = MessageBrokerSubscriptionState.Disposing;
    }

    internal void EndDisposing()
    {
        using ( AcquireLock() )
        {
            Assume.Equals( _state, MessageBrokerSubscriptionState.Disposing );
            _state = MessageBrokerSubscriptionState.Disposed;
        }

        Emit( MessageBrokerSubscriptionEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerSubscriptionEvent e)
    {
        if ( _eventHandler is null )
            return;

        try
        {
            _eventHandler( e );
        }
        catch
        {
            // NOTE: do nothing
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Dispose(bool notifyChannel)
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerSubscriptionState.Disposing;
        }

        Emit( MessageBrokerSubscriptionEvent.Disposing( this ) );

        if ( notifyChannel )
            Channel.OnSubscriptionDisposing( Client );

        EndDisposing();
    }
}
