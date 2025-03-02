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

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerChannel"/> instances to which a client is linked to.
/// </summary>
public readonly struct MessageBrokerRemoteClientLinkedChannelCollection
{
    private readonly MessageBrokerRemoteClient _client;

    internal MessageBrokerRemoteClientLinkedChannelCollection(MessageBrokerRemoteClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of linked channels.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _client.AcquireLock() )
                return _client.LinkedChannelsById.Count;
        }
    }

    /// <summary>
    /// Returns all linked channels.
    /// </summary>
    /// <returns>All linked channels.</returns>
    [Pure]
    public MessageBrokerChannel[] GetAll()
    {
        using ( _client.AcquireLock() )
        {
            if ( _client.LinkedChannelsById.Count == 0 )
                return Array.Empty<MessageBrokerChannel>();

            var i = 0;
            var result = new MessageBrokerChannel[_client.LinkedChannelsById.Count];
            foreach ( var client in _client.LinkedChannelsById.Values )
                result[i++] = client;

            return result;
        }
    }

    /// <summary>
    /// Attempts to return a linked channel with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannel"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannel? TryGetById(int id)
    {
        using ( _client.AcquireLock() )
            return _client.LinkedChannelsById.TryGetValue( id, out var result ) ? result : null;
    }
}
