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
/// Represents a collection of <see cref="MessageBrokerChannel"/> instances.
/// </summary>
public readonly struct MessageBrokerChannelCollection
{
    private readonly MessageBrokerServer _server;

    internal MessageBrokerChannelCollection(MessageBrokerServer server)
    {
        _server = server;
    }

    /// <summary>
    /// Specifies the number of owned channels.
    /// </summary>
    public int Count => ChannelCollection.GetCount( _server );

    /// <summary>
    /// Returns all owned channels.
    /// </summary>
    /// <returns>All owned channels.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerChannel> GetAll()
    {
        return ChannelCollection.GetAll( _server );
    }

    /// <summary>
    /// Attempts to return a channel with the provided <paramref name="id"/>.
    /// </summary>
    /// <param name="id">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannel"/> instance associated with the provided <paramref name="id"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannel? TryGetById(int id)
    {
        return ChannelCollection.TryGetById( _server, id );
    }

    /// <summary>
    /// Attempts to return a channel with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Channel's unique <see cref="MessageBrokerChannel.Name"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannel"/> instance associated with the provided <paramref name="name"/>
    /// or <b>null</b>, when such a channel does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannel? TryGetByName(string name)
    {
        return ChannelCollection.TryGetByName( _server, name );
    }
}
