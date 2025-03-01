using System.Diagnostics.Contracts;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerChannel"/> instances.
/// </summary>
public readonly struct MessageBrokerChannelCollection
{
    private readonly MessageBrokerServer _server;

    internal MessageBrokerChannelCollection(MessageBrokerServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Specifies the number of owned channels.
    /// </summary>
    public int Count => ChannelCollection.GetCount( _server );

    /// <summary>
    /// Returns all owned channels.
    /// </summary>
    /// <returns>All owned channels.</returns>
    [Pure]
    public MessageBrokerChannel[] GetAll()
    {
        return ChannelCollection.GetAll( _server );
    }

    /// <summary>
    /// Attempts to return a channel with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannel"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannel? TryGetById(int id)
    {
        return ChannelCollection.TryGetById( _server, id );
    }

    /// <summary>
    /// Attempts to return a channel with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Channel's unique <see cref="MessageBrokerChannel.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannel"/> instance associated with the provided <paramref name="name"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannel? TryGetByName(string name)
    {
        return ChannelCollection.TryGetByName( _server, name );
    }
}
