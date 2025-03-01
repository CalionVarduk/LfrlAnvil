using System.Runtime.CompilerServices;
using LfrlAnvil.Async;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a message broker channel linked to the client, which allows to publish messages.
/// </summary>
public sealed class MessageBrokerLinkedChannel
{
    private readonly object _sync = new object();
    private MessageBrokerLinkedChannelState _state;

    internal MessageBrokerLinkedChannel(MessageBrokerClient client, int id, string name)
    {
        Client = client;
        Id = id;
        Name = name;
        _state = MessageBrokerLinkedChannelState.Linked;
    }

    /// <summary>
    /// <see cref="MessageBrokerClient"/> instance to which this channel is linked to.
    /// </summary>
    public MessageBrokerClient Client { get; }

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
    /// <remarks>See <see cref="MessageBrokerLinkedChannelState"/> for more information.</remarks>
    public MessageBrokerLinkedChannelState State
    {
        get
        {
            using ( AcquireLock() )
                return _state;
        }
    }

    // TODO: add UnlinkAsync method

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void OnClientDisposed()
    {
        using ( AcquireLock() )
            _state = MessageBrokerLinkedChannelState.Unlinked;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ExclusiveLock AcquireLock()
    {
        return ExclusiveLock.Enter( _sync );
    }
}
