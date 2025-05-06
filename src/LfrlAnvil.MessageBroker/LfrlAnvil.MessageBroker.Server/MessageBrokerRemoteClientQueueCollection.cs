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
/// Represents a collection of <see cref="MessageBrokerQueue"/> instances attached to a single client, identified by their names.
/// </summary>
public readonly struct MessageBrokerRemoteClientQueueCollection
{
    private readonly MessageBrokerRemoteClient _client;

    internal MessageBrokerRemoteClientQueueCollection(MessageBrokerRemoteClient client)
    {
        _client = client;
    }

    /// <summary>
    /// Specifies the number of queues.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _client.AcquireLock() )
                return _client.QueuesByName.Count;
        }
    }

    /// <summary>
    /// Returns all queues.
    /// </summary>
    /// <returns>All queues.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerQueue> GetAll()
    {
        using ( _client.AcquireLock() )
            return _client.QueuesByName.GetAll();
    }

    /// <summary>
    /// Attempts to return a queue by its id.
    /// </summary>
    /// <param name="id">Queue's unique <see cref="MessageBrokerQueue.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueue"/> instance associated with the client and the provided <paramref name="id"/>
    /// or <b>null</b>, when such a queue does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueue? TryGetById(int id)
    {
        using ( _client.AcquireLock() )
            return _client.QueuesByName.TryGetById( id );
    }

    /// <summary>
    /// Attempts to return a queue by its name.
    /// </summary>
    /// <param name="name">Queue's unique <see cref="MessageBrokerQueue.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueue"/> instance associated with the client and the provided <paramref name="name"/>
    /// or <b>null</b>, when such a queue does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueue? TryGetByName(string name)
    {
        using ( _client.AcquireLock() )
            return _client.QueuesByName.TryGetByName( name );
    }
}
