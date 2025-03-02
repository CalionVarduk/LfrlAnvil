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

using System.Collections.Generic;
using System.Runtime.CompilerServices;
using LfrlAnvil.Async;
using LfrlAnvil.MessageBroker.Server.Events;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a message broker channel, which allows clients to publish messages.
/// </summary>
public sealed class MessageBrokerChannel
{
    internal readonly Dictionary<int, MessageBrokerRemoteClient> LinkedClientsById;
    private readonly MessageBrokerChannelEventHandler? _eventHandler;
    private MessageBrokerChannelState _state;

    internal MessageBrokerChannel(MessageBrokerServer server, int id, string name)
    {
        Server = server;
        Id = id;
        Name = name;
        _state = MessageBrokerChannelState.Running;
        LinkedClientsById = new Dictionary<int, MessageBrokerRemoteClient>();
        _eventHandler = Server.ChannelEventHandlerFactory?.Invoke( this );
    }

    /// <summary>
    /// <see cref="MessageBrokerServer"/> instance to which this channel belongs to.
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
    /// Collection of <see cref="MessageBrokerRemoteClient"/> instances linked to this channel.
    /// </summary>
    public MessageBrokerChannelLinkedClientCollection LinkedClients => new MessageBrokerChannelLinkedClientCollection( this );

    internal bool ShouldCancel => _state >= MessageBrokerChannelState.Disposing;

    internal enum UnlinkResult : byte
    {
        NoChanges = 0,
        Unlinked = 1,
        Disposing = 2
    }

    internal UnlinkResult BeginUnlink(MessageBrokerRemoteClient client)
    {
        if ( ShouldCancel || ! LinkedClientsById.Remove( client.Id ) )
            return UnlinkResult.NoChanges;

        UnlinkResult result;
        if ( LinkedClientsById.Count == 0 )
        {
            result = UnlinkResult.Disposing;
            _state = MessageBrokerChannelState.Disposing;
        }
        else
            result = UnlinkResult.Unlinked;

        return result;
    }

    internal void DisposeDueToLackOfReferences()
    {
        Assume.IsEmpty( LinkedClientsById );
        Assume.Equals( State, MessageBrokerChannelState.Disposing );

        Emit( MessageBrokerChannelEvent.Disposing( this ) );

        var exc = ChannelCollection.Remove( this ).Exception;
        if ( exc is not null )
            Emit( MessageBrokerChannelEvent.Unexpected( this, exc ) );

        using ( AcquireLock() )
            _state = MessageBrokerChannelState.Disposed;

        Emit( MessageBrokerChannelEvent.Disposed( this ) );
    }

    internal void OnClientDisconnected(MessageBrokerRemoteClient client)
    {
        UnlinkResult result;
        using ( AcquireLock() )
            result = BeginUnlink( client );

        if ( result == UnlinkResult.NoChanges )
            return;

        Emit( MessageBrokerChannelEvent.Unlinked( this, client ) );
        if ( result == UnlinkResult.Disposing )
            DisposeDueToLackOfReferences();
    }

    internal void OnServerDisposed()
    {
        using ( AcquireLock() )
        {
            if ( ShouldCancel )
                return;

            _state = MessageBrokerChannelState.Disposing;
        }

        Emit( MessageBrokerChannelEvent.Disposing( this ) );

        using ( AcquireLock() )
        {
            LinkedClientsById.Clear();
            _state = MessageBrokerChannelState.Disposed;
        }

        Emit( MessageBrokerChannelEvent.Disposed( this ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( LinkedClientsById );
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
