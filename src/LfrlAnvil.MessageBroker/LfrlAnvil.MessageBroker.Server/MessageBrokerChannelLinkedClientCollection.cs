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
