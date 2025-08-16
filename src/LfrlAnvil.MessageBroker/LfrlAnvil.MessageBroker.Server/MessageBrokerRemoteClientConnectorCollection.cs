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
/// Represents a collection of <see cref="MessageBrokerRemoteClientConnector"/> instances.
/// </summary>
public readonly struct MessageBrokerRemoteClientConnectorCollection
{
    private readonly MessageBrokerServer _server;

    internal MessageBrokerRemoteClientConnectorCollection(MessageBrokerServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Specifies the number of owned connectors.
    /// </summary>
    public int Count => RemoteClientConnectorCollection.GetCount( _server );

    /// <summary>
    /// Returns all owned connectors.
    /// </summary>
    /// <returns>All owned connectors.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerRemoteClientConnector> GetAll()
    {
        return RemoteClientConnectorCollection.GetAll( _server );
    }

    /// <summary>
    /// Attempts to return a connector with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Connector's unique <see cref="MessageBrokerRemoteClientConnector.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerRemoteClientConnector"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a connector does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerRemoteClientConnector? TryGetById(int id)
    {
        return RemoteClientConnectorCollection.TryGetById( _server, id );
    }
}
