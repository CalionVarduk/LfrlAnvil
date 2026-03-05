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
    public ReadOnlyArray<MessageBrokerListener> GetAll()
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
    /// Attempts to bind the client to a channel as a message listener.
    /// </summary>
    /// <param name="channelName">Unique name of the channel to bind as listener to.</param>
    /// <param name="callback">Callback invoked when the created listener receives a message from the server.</param>
    /// <param name="queueName">
    /// Optional unique name of the queue that will store pending messages to this client server-side.
    /// Equal to the provided <paramref name="channelName"/> by default.
    /// </param>
    /// <param name="options">Optional creation options. Equal to <see cref="MessageBrokerListenerOptions.Default"/> by default.</param>
    /// <param name="createChannelIfNotExists">
    /// Specifies whether the server should create the channel if it does not exist yet. Equal to <b>true</b> by default.
    /// </param>
    /// <param name="isEphemeral">Specifies whether the listener will be ephemeral. Equal to <b>false</b> by default.</param>
    /// <returns>
    /// A task that represents the operation, which returns a <see cref="Result{T}"/> instance,
    /// with underlying <see cref="MessageBrokerBindListenerResult"/> instance.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="channelName"/> or <paramref name="queueName"/> (if not <b>null</b>) length
    /// is less than <b>1</b> or greater than <b>512</b>.
    /// </exception>
    /// <exception cref="MessageBrokerClientDisposedException">When client has already been disposed.</exception>
    /// <exception cref="MessageBrokerClientStateException">
    /// When client is not disposed and not in <see cref="MessageBrokerClientState.Running"/> state.
    /// </exception>
    /// <remarks>
    /// Unexpected errors encountered during listener binding attempt will cause the client to be automatically disposed.
    /// Returned <see cref="Result{T}"/> will only be valid when either the client has been successfully bound as listener to the channel
    /// on the server side, or the client is already locally bound as listener to the channel, which will cancel the request to the server.
    /// </remarks>
    public ValueTask<Result<MessageBrokerBindListenerResult?>> BindAsync(
        string channelName,
        MessageBrokerListenerCallback callback,
        MessageBrokerListenerOptions options = default,
        string? queueName = null,
        bool createChannelIfNotExists = true,
        bool isEphemeral = false)
    {
        return ListenerCollection.BindAsync( _client, channelName, queueName, callback, options, createChannelIfNotExists, isEphemeral );
    }
}
