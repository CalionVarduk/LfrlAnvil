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
/// Represents a collection of <see cref="MessageBrokerChannelListenerBinding"/> instances attached to a single client,
/// identified by channel ids.
/// </summary>
public readonly struct MessageBrokerRemoteClientListenerCollection
{
    private readonly MessageBrokerRemoteClient _client;

    internal MessageBrokerRemoteClientListenerCollection(MessageBrokerRemoteClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of listeners.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _client.AcquireLock() )
                return _client.ListenersByChannelId.Count;
        }
    }

    /// <summary>
    /// Returns all listeners.
    /// </summary>
    /// <returns>All listeners.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerChannelListenerBinding> GetAll()
    {
        using ( _client.AcquireLock() )
            return _client.ListenersByChannelId.GetAll();
    }

    /// <summary>
    /// Attempts to return a listener by related channel id.
    /// </summary>
    /// <param name="channelId">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannelListenerBinding"/> instance associated with the client and the provided <paramref name="channelId"/>
    /// or <b>null</b>, when such a listener does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannelListenerBinding? TryGetByChannelId(int channelId)
    {
        using ( _client.AcquireLock() )
            return _client.ListenersByChannelId.TryGet( channelId, out var result ) ? result : null;
    }
}
