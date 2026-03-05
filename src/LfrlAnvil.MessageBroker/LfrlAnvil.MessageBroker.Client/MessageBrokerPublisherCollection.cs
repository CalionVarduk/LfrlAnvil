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

using System;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerPublisher"/> instances.
/// </summary>
public readonly struct MessageBrokerPublisherCollection
{
    private readonly MessageBrokerClient _client;

    internal MessageBrokerPublisherCollection(MessageBrokerClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of publishers.
    /// </summary>
    public int Count => PublisherCollection.GetCount( _client );

    /// <summary>
    /// Returns all publishers.
    /// </summary>
    /// <returns>All publishers.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerPublisher> GetAll()
    {
        return PublisherCollection.GetAll( _client );
    }

    /// <summary>
    /// Attempts to return a publisher by related channel id.
    /// </summary>
    /// <param name="channelId">Unique id of the channel to which the publisher is related.</param>
    /// <returns>
    /// <see cref="MessageBrokerPublisher"/> instance associated with the provided <paramref name="channelId"/>
    /// or <b>null</b>, when such a publisher does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerPublisher? TryGetByChannelId(int channelId)
    {
        return PublisherCollection.TryGetByChannelId( _client, channelId );
    }

    /// <summary>
    /// Attempts to return a publisher by related channel name.
    /// </summary>
    /// <param name="channelName">Unique name of the channel to which the publisher is related.</param>
    /// <returns>
    /// <see cref="MessageBrokerPublisher"/> instance associated with the provided <paramref name="channelName"/>
    /// or <b>null</b>, when such a publisher does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerPublisher? TryGetByChannelName(string channelName)
    {
        return PublisherCollection.TryGetByChannelName( _client, channelName );
    }

    /// <summary>
    /// Attempts to bind the client to a channel as a message publisher.
    /// </summary>
    /// <param name="channelName">Unique name of the channel to bind as publisher to.</param>
    /// <param name="streamName">
    /// Optional unique name of the stream to which to push messages. Equal to the provided <paramref name="channelName"/> by default.
    /// </param>
    /// <param name="isEphemeral">Specifies whether the publisher will be ephemeral. Equal to <b>false</b> by default.</param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerBindPublisherResult"/> instance.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="channelName"/> or <paramref name="streamName"/> (if not <b>null</b>) length
    /// is less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientStateException">
    /// When client is not disposed and not in <see cref="MessageBrokerClientState.Running"/> state.
    /// </exception>
    /// <remarks>
    /// Unexpected errors encountered during publisher binding attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has been successfully bound as publisher to the channel
    /// on the server side, or the client is already locally bound as publisher to the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerBindPublisherResult?>> BindAsync(
        string channelName,
        string? streamName = null,
        bool isEphemeral = false)
    {
        return PublisherCollection.BindAsync( _client, channelName, streamName, isEphemeral );
    }
}
