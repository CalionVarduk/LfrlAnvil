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

    internal void OnClientDisconnected(MessageBrokerRemoteClient client)
    {
        bool dispose;
        using ( AcquireLock() )
        {
            if ( ShouldCancel || ! LinkedClientsById.Remove( client.Id ) )
                return;

            dispose = LinkedClientsById.Count == 0;
            if ( dispose )
                _state = MessageBrokerChannelState.Disposing;
        }

        Emit( MessageBrokerChannelEvent.Unlinked( this, client ) );
        if ( ! dispose )
            return;

        Emit( MessageBrokerChannelEvent.Disposing( this ) );

        var exc = ChannelCollection.Remove( this ).Exception;
        if ( exc is not null )
            Emit( MessageBrokerChannelEvent.Unexpected( this, exc ) );

        using ( AcquireLock() )
            _state = MessageBrokerChannelState.Disposed;

        Emit( MessageBrokerChannelEvent.Disposed( this ) );
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
