using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerRemoteClient"/> instances linked to a channel.
/// </summary>
public readonly struct MessageBrokerChannelLinkedClientCollection
{
    private readonly MessageBrokerChannel _channel;

    internal MessageBrokerChannelLinkedClientCollection(MessageBrokerChannel channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// Specifies the number of linked clients.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _channel.AcquireLock() )
                return _channel.LinkedClientsById.Count;
        }
    }

    /// <summary>
    /// Returns all linked clients.
    /// </summary>
    /// <returns>All linked clients.</returns>
    [Pure]
    public MessageBrokerRemoteClient[] GetAll()
    {
        using ( _channel.AcquireLock() )
        {
            if ( _channel.LinkedClientsById.Count == 0 )
                return Array.Empty<MessageBrokerRemoteClient>();

            var i = 0;
            var result = new MessageBrokerRemoteClient[_channel.LinkedClientsById.Count];
            foreach ( var client in _channel.LinkedClientsById.Values )
                result[i++] = client;

            return result;
        }
    }

    /// <summary>
    /// Attempts to return a linked client with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Client's unique <see cref="MessageBrokerRemoteClient.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerRemoteClient"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a client does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerRemoteClient? TryGetById(int id)
    {
        using ( _channel.AcquireLock() )
            return _channel.LinkedClientsById.TryGetValue( id, out var result ) ? result : null;
    }
}
