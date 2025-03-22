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
/// Represents a message broker channel binding, which allows clients to publish messages through channels.
/// </summary>
public sealed class MessageBrokerChannelBinding
{
    private readonly object _sync = new object();
    private readonly MessageBrokerChannelBindingEventHandler? _eventHandler;
    private MessageBrokerChannelBindingState _state;

    internal MessageBrokerChannelBinding(MessageBrokerRemoteClient client, MessageBrokerChannel channel)
    {
        Client = client;
        Channel = channel;
        _state = MessageBrokerChannelBindingState.Running;
        _eventHandler = client.Server.ChannelBindingEventHandlerFactory?.Invoke( this );
    }

    /// <summary>
    /// <see cref="MessageBrokerRemoteClient"/> instance to which this binding belongs to.
    /// </summary>
    public MessageBrokerRemoteClient Client { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannel"/> instance to which the <see cref="Client"/> is bound to.
    /// </summary>
    public MessageBrokerChannel Channel { get; }

    /// <summary>
    /// Current binding's state.
    /// </summary>
    /// <remarks>See <see cref="MessageBrokerChannelBindingState"/> for more information.</remarks>
    public MessageBrokerChannelBindingState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    internal bool ShouldCancel => _state >= MessageBrokerChannelBindingState.Disposing;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerChannelBinding"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Client.Id}] '{Client.Name}' => [{Channel.Id}] '{Channel.Name}' binding ({State})";
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
        Assume.Equals( _state, MessageBrokerChannelBindingState.Running );
        _state = MessageBrokerChannelBindingState.Disposing;
    }

    internal void EndDisposing()
    {
        using ( AcquireLock() )
        {
            Assume.Equals( _state, MessageBrokerChannelBindingState.Disposing );
            _state = MessageBrokerChannelBindingState.Disposed;
        }

        Emit( MessageBrokerChannelBindingEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Emit(MessageBrokerChannelBindingEvent e)
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

            _state = MessageBrokerChannelBindingState.Disposing;
        }

        Emit( MessageBrokerChannelBindingEvent.Disposing( this ) );

        if ( notifyChannel )
            Channel.OnBindingDisposing( Client );

        EndDisposing();
    }
}
