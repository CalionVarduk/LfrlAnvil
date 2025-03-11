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
/// Represents a collection of <see cref="MessageBrokerChannelBinding"/> instances attached to a single client, identified by channel ids.
/// </summary>
public readonly struct MessageBrokerRemoteClientBindingCollection
{
    private readonly MessageBrokerRemoteClient _client;

    internal MessageBrokerRemoteClientBindingCollection(MessageBrokerRemoteClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of bindings.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _client.AcquireLock() )
                return _client.BindingsByChannelId.Count;
        }
    }

    /// <summary>
    /// Returns all bindings.
    /// </summary>
    /// <returns>All bindings.</returns>
    [Pure]
    public MessageBrokerChannelBinding[] GetAll()
    {
        using ( _client.AcquireLock() )
        {
            if ( _client.BindingsByChannelId.Count == 0 )
                return Array.Empty<MessageBrokerChannelBinding>();

            var i = 0;
            var result = new MessageBrokerChannelBinding[_client.BindingsByChannelId.Count];
            foreach ( var binding in _client.BindingsByChannelId.Values )
                result[i++] = binding;

            return result;
        }
    }

    /// <summary>
    /// Attempts to return a binding by related channel id.
    /// </summary>
    /// <param name="channelId">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannelBinding"/> instance associated with the client and the provided <paramref name="channelId"/>
    /// or <b>null</b>, when such a binding does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannelBinding? TryGetByChannelId(int channelId)
    {
        using ( _client.AcquireLock() )
            return _client.BindingsByChannelId.TryGetValue( channelId, out var result ) ? result : null;
    }
}
