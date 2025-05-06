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
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel, which allows clients to publish messages.
/// </summary>
public sealed class MessageBrokerChannel
{
    internal ReferenceStore<int, MessageBrokerChannelBinding> BindingsByClientId;
    internal ReferenceStore<int, MessageBrokerSubscription> SubscriptionsByClientId;
    private readonly object _sync = new object();
    private readonly MessageBrokerChannelEventHandler? _eventHandler;
    private MessageBrokerChannelState _state;

    internal MessageBrokerChannel(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _state = MessageBrokerChannelState.Running;
        BindingsByClientId = ReferenceStore<int, MessageBrokerChannelBinding>.Create();
        SubscriptionsByClientId = ReferenceStore<int, MessageBrokerSubscription>.Create();
        _eventHandler = Server.ChannelEventHandlerFactory?.Invoke( this );
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance that owns this channel.
    /// </summary>
    public MessageBrokerServer Server { get; }

    /// <summary>
    /// Channel's unique identifier assigned by the server.
    /// </summary>
    public int Id { get; }

    /// <summary>
    /// Channel's unique name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Current channel's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelState"/> for more information.</remarks>
    public MessageBrokerChannelState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    /// <summary>
    /// Collection of <see cref="MessageBrokerChannelBinding"/> instances attached to this channel, identified by client ids.
    /// </summary>
    public MessageBrokerChannelBindingCollection Bindings => new MessageBrokerChannelBindingCollection( this );

    /// <summary>
    /// Collection of <see cref="MessageBrokerSubscription"/> instances attached to this channel, identified by client ids.
    /// </summary>
    public MessageBrokerChannelSubscriptionCollection Subscriptions => new MessageBrokerChannelSubscriptionCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerChannelState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannel"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Id}] '{Name}' channel ({State})";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingBindingUnsafe(int clientId)
    {
        BindingsByClientId.Remove( clientId );
        if ( SubscriptionsByClientId.Count > 0 || BindingsByClientId.Count > 0 )
            return false;

        _state = MessageBrokerChannelState.Disposing;
        return true;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal bool TryDisposeByRemovingSubscriptionUnsafe(int clientId)
    {
        SubscriptionsByClientId.Remove( clientId );
        if ( SubscriptionsByClientId.Count > 0 || BindingsByClientId.Count > 0 )
            return false;

        _state = MessageBrokerChannelState.Disposing;
        return true;
    }

    internal void OnBindingDisposing(MessageBrokerRemoteClient client)
    {
        var dispose = false;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            if ( BindingsByClientId.Remove( client.Id ) && BindingsByClientId.Count == 0 && SubscriptionsByClientId.Count == 0 )
            {
                dispose = true;
                _state = MessageBrokerChannelState.Disposing;
            }
        }

        if ( dispose )
            DisposeDueToLackOfReferences();
    }

    internal void OnSubscriptionDisposing(MessageBrokerRemoteClient client)
    {
        var dispose = false;
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            if ( SubscriptionsByClientId.Remove( client.Id ) && BindingsByClientId.Count == 0 && SubscriptionsByClientId.Count == 0 )
            {
                dispose = true;
                _state = MessageBrokerChannelState.Disposing;
            }
        }

        if ( dispose )
            DisposeDueToLackOfReferences();
    }

    internal async ValueTask OnServerDisposedAsync()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelState.Disposing;
        }

        Emit( MessageBrokerChannelEvent.Disposing( this ) );

        MessageBrokerChannelBinding[] bindings;
        MessageBrokerSubscription[] subscriptions;
        using ( AcquireLock() )
        {
            bindings = BindingsByClientId.ClearAndExtract();
            subscriptions = SubscriptionsByClientId.ClearAndExtract();
            _state = MessageBrokerChannelState.Disposed;
        }

        await Parallel.ForEachAsync( bindings, static (b, _) => b.OnServerDisposedAsync() ).ConfigureAwait( false );
        foreach ( var subscription in subscriptions )
            subscription.OnServerDisposed();

        Emit( MessageBrokerChannelEvent.Disposed( this ) );
    }

    internal void DisposeDueToLackOfReferences()
    {
        Assume.Equals( State, MessageBrokerChannelState.Disposing );

        Emit( MessageBrokerChannelEvent.Disposing( this ) );

        var exc = ChannelCollection.Remove( this ).Exception;
        if ( exc is not null )
            Emit( MessageBrokerChannelEvent.Unexpected( this, exc ) );

        using ( AcquireLock() )
        {
            Assume.Equals( BindingsByClientId.Count, 0 );
            Assume.Equals( SubscriptionsByClientId.Count, 0 );
            _state = MessageBrokerChannelState.Disposed;
        }

        Emit( MessageBrokerChannelEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.SpinWaitEnter( _sync, spinWaitMultiplier: 4 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerChannelEvent e)
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
}
