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
using System.Threading.Tasks;
using LfrlAnvil.MessageBroker.Client.Exceptions;
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerListener"/> instances.
/// </summary>
public readonly struct MessageBrokerListenerCollection
{
    private readonly MessageBrokerClient _client;

    internal MessageBrokerListenerCollection(MessageBrokerClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of listeners.
    /// </summary>
    public int Count => ListenerCollection.GetCount( _client );

    /// <summary>
    /// Returns all listeners.
    /// </summary>
    /// <returns>All listeners.</returns>
    [Pure]
    public MessageBrokerListener[] GetAll()
    {
        return ListenerCollection.GetAll( _client );
    }

    /// <summary>
    /// Attempts to return a listener by related channel id.
    /// </summary>
    /// <param name="channelId">Unique id of the channel to which the listener is related.</param>
    /// <returns>
    /// <see cref="MessageBrokerListener"/> instance associated with the provided <paramref name="channelId"/>
    /// or <b>null</b>, when such a listener does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerListener? TryGetByChannelId(int channelId)
    {
        return ListenerCollection.TryGetByChannelId( _client, channelId );
    }

    /// <summary>
    /// Attempts to return a listener by related channel name.
    /// </summary>
    /// <param name="channelName">Unique name of the channel to which the listener is related.</param>
    /// <returns>
    /// <see cref="MessageBrokerListener"/> instance associated with the provided <paramref name="channelName"/>
    /// or <b>null</b>, when such a listener does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerListener? TryGetByChannelName(string channelName)
    {
        return ListenerCollection.TryGetByChannelName( _client, channelName );
    }

    /// <summary>
    /// Attempts to subscribe the client to a channel.
    /// </summary>
    /// <param name="channelName">Unique name of the channel to subscribe to.</param>
    /// <param name="createChannelIfNotExists">
    /// Specifies whether or not the server should create the channel if it does not exist yet. Equal to <b>true</b> by default.
    /// </param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerSubscriptionResult"/> instance.
    /// </returns>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientStateException">
    /// When client is not disposed and not in <see cref="MessageBrokerClientState.Running"/> state.
    /// </exception>
    /// <remarks>
    /// Unexpected errors encountered during subscription attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has successfully subscribed to the channel
    /// on the server side, or the client is already locally subscribed to the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerSubscriptionResult?>> SubscribeAsync(string channelName, bool createChannelIfNotExists = true)
    {
        return ListenerCollection.SubscribeAsync( _client, channelName, createChannelIfNotExists );
    }
}
