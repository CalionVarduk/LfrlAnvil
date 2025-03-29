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
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of <see cref="MessageBrokerQueue"/> instances.
/// </summary>
public readonly struct MessageBrokerQueueCollection
{
    private readonly MessageBrokerServer _server;

    internal MessageBrokerQueueCollection(MessageBrokerServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Specifies the number of owned queues.
    /// </summary>
    public int Count => QueueCollection.GetCount( _server );

    /// <summary>
    /// Returns all owned queues.
    /// </summary>
    /// <returns>All owned queues.</returns>
    [Pure]
    public MessageBrokerQueue[] GetAll()
    {
        return QueueCollection.GetAll( _server );
    }

    /// <summary>
    /// Attempts to return a queue with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Queue's unique <see cref="MessageBrokerQueue.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueue"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a queue does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueue? TryGetById(int id)
    {
        return QueueCollection.TryGetById( _server, id );
    }

    /// <summary>
    /// Attempts to return a queue with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Queue's unique <see cref="MessageBrokerQueue.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueue"/> instance associated with the provided <paramref name="name"/>
    /// or <b>null</b>, when such a queue does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueue? TryGetByName(string name)
    {
        return QueueCollection.TryGetByName( _server, name );
    }
}
