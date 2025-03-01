using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerLinkedChannel"/> instances.
/// </summary>
public readonly struct MessageBrokerLinkedChannelCollection
{
    private readonly MessageBrokerClient _client;

    internal MessageBrokerLinkedChannelCollection(MessageBrokerClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of linked channels.
    /// </summary>
    public int Count => LinkedChannelCollection.GetCount( _client );

    /// <summary>
    /// Returns all linked channels.
    /// </summary>
    /// <returns>All linked channels.</returns>
    [Pure]
    public MessageBrokerLinkedChannel[] GetAll()
    {
        return LinkedChannelCollection.GetAll( _client );
    }

    /// <summary>
    /// Attempts to return a channel with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Channel's unique <see cref="MessageBrokerLinkedChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerLinkedChannel"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerLinkedChannel? TryGetById(int id)
    {
        return LinkedChannelCollection.TryGetById( _client, id );
    }

    /// <summary>
    /// Attempts to return a channel with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Channel's unique <see cref="MessageBrokerLinkedChannel.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerLinkedChannel"/> instance associated with the provided <paramref name="name"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerLinkedChannel? TryGetByName(string name)
    {
        return LinkedChannelCollection.TryGetByName( _client, name );
    }

    /// <summary>
    /// Attempts to link a channel to the client.
    /// </summary>
    /// <param name="name">Unique name of the channel to link.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerChannelLinkResult"/> instance.
    /// </returns>
    /// <exception cref="OperationCanceledException">When <paramref name="cancellationToken"/> has been cancelled.</exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientStateException">
    /// When client is not disposed and not in <see cref="MessageBrokerClientState.Running"/> state.
    /// </exception>
    /// <remarks>
    /// Unexpected errors encountered during channel linkage will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the channel has been successfully linked to the client
    /// on the server side, or the channel is already locally linked to the client, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerChannelLinkResult?>> LinkAsync(
        string name,
        CancellationToken cancellationToken = default)
    {
        return LinkedChannelCollection.LinkAsync( _client, name, cancellationToken );
    }
}
