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
/// Represents a collection of <see cref="MessageBrokerRemoteClient"/> instances.
/// </summary>
public readonly struct MessageBrokerRemoteClientCollection
{
    private readonly MessageBrokerServer _server;

    internal MessageBrokerRemoteClientCollection(MessageBrokerServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Specifies the number of owned clients.
    /// </summary>
    public int Count => RemoteClientCollection.GetCount( _server );

    /// <summary>
    /// Returns all owned clients.
    /// </summary>
    /// <returns>All owned clients.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerRemoteClient> GetAll()
    {
        return RemoteClientCollection.GetAll( _server );
    }

    /// <summary>
    /// Attempts to return a client with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Client's unique <see cref="MessageBrokerRemoteClient.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerRemoteClient"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a client does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerRemoteClient? TryGetById(int id)
    {
        return RemoteClientCollection.TryGetById( _server, id );
    }

    /// <summary>
    /// Attempts to return a client with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Client's unique <see cref="MessageBrokerRemoteClient.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerRemoteClient"/> instance associated with the provided <paramref name="name"/>
    /// or <b>null</b>, when such a client does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerRemoteClient? TryGetByName(string name)
    {
        return RemoteClientCollection.TryGetByName( _server, name );
    }
}
