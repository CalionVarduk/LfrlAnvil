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
/// Represents a collection of <see cref="MessageBrokerStream"/> instances.
/// </summary>
public readonly struct MessageBrokerStreamCollection
{
    private readonly MessageBrokerServer _server;

    internal MessageBrokerStreamCollection(MessageBrokerServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Specifies the number of owned streams.
    /// </summary>
    public int Count => StreamCollection.GetCount( _server );

    /// <summary>
    /// Returns all owned streams.
    /// </summary>
    /// <returns>All owned streams.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerStream> GetAll()
    {
        return StreamCollection.GetAll( _server );
    }

    /// <summary>
    /// Attempts to return a stream with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Stream's unique <see cref="MessageBrokerStream.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerStream"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a stream does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerStream? TryGetById(int id)
    {
        return StreamCollection.TryGetById( _server, id );
    }

    /// <summary>
    /// Attempts to return a stream with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Stream's unique <see cref="MessageBrokerStream.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerStream"/> instance associated with the provided <paramref name="name"/>
    /// or <b>null</b>, when such a stream does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerStream? TryGetByName(string name)
    {
        return StreamCollection.TryGetByName( _server, name );
    }
}
