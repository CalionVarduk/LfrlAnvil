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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerChannelPublisherBinding"/> instances attached to a single stream,
/// identified by (client-id, channel-id) tuples.
/// </summary>
public readonly struct MessageBrokerStreamPublisherCollection
{
    private readonly MessageBrokerStream _stream;

    internal MessageBrokerStreamPublisherCollection(MessageBrokerStream stream)
    {
        _stream = stream;
    }

    /// <summary>
    /// Specifies the number of publishers.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _stream.AcquireLock() )
                return _stream.PublishersByClientChannelIdPair.Count;
        }
    }

    /// <summary>
    /// Returns all publishers.
    /// </summary>
    /// <returns>All publishers.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerChannelPublisherBinding> GetAll()
    {
        using ( _stream.AcquireLock() )
            return _stream.PublishersByClientChannelIdPair.GetAll();
    }

    /// <summary>
    /// Attempts to return a publisher by related (client-id, channel-id) tuple.
    /// </summary>
    /// <param name="clientId">Client's unique <see cref="MessageBrokerRemoteClient.Id"/>.</param>
    /// <param name="channelId">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> instance associated with the stream and the provided
    /// (<paramref name="clientId"/>, <paramref name="channelId"/>) tuple or <b>null</b>, when such a publisher does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannelPublisherBinding? TryGetByKey(int clientId, int channelId)
    {
        using ( _stream.AcquireLock() )
            return _stream.PublishersByClientChannelIdPair.TryGet( new Pair<int, int>( clientId, channelId ), out var result )
                ? result
                : null;
    }
}
