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
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerChannelBinding"/> instances attached to a single queue,
/// identified by (client-id, channel-id) tuples.
/// </summary>
public readonly struct MessageBrokerQueueBindingCollection
{
    private readonly MessageBrokerQueue _queue;

    internal MessageBrokerQueueBindingCollection(MessageBrokerQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// Specifies the number of bindings.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _queue.AcquireLock() )
                return _queue.BindingsByKey.Count;
        }
    }

    /// <summary>
    /// Returns all bindings.
    /// </summary>
    /// <returns>All bindings.</returns>
    [Pure]
    public MessageBrokerChannelBinding[] GetAll()
    {
        using ( _queue.AcquireLock() )
        {
            if ( _queue.BindingsByKey.Count == 0 )
                return Array.Empty<MessageBrokerChannelBinding>();

            var i = 0;
            var result = new MessageBrokerChannelBinding[_queue.BindingsByKey.Count];
            foreach ( var binding in _queue.BindingsByKey.Values )
                result[i++] = binding;

            return result;
        }
    }

    /// <summary>
    /// Attempts to return a binding by related (client-id, channel-id) tuple.
    /// </summary>
    /// <param name="clientId">Client's unique <see cref="MessageBrokerRemoteClient.Id"/>.</param>
    /// <param name="channelId">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannelBinding"/> instance associated with the queue and the provided
    /// (<paramref name="clientId"/>, <paramref name="channelId"/>) tuple or <b>null</b>, when such a binding does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannelBinding? TryGetByKey(int clientId, int channelId)
    {
        using ( _queue.AcquireLock() )
            return _queue.BindingsByKey.TryGetValue( new QueueBindingKey( clientId, channelId ), out var result ) ? result : null;
    }
}
