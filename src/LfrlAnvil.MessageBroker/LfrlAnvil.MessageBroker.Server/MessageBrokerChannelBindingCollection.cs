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
/// Represents a collection of <see cref="MessageBrokerChannelBinding"/> instances attached to a single channel, identified by client ids.
/// </summary>
public readonly struct MessageBrokerChannelBindingCollection
{
    private readonly MessageBrokerChannel _channel;

    internal MessageBrokerChannelBindingCollection(MessageBrokerChannel channel)
    {
        _channel = channel;
    }

    /// <summary>
    /// Specifies the number of bindings.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _channel.AcquireLock() )
                return _channel.BindingsByClientId.Count;
        }
    }

    /// <summary>
    /// Returns all bindings.
    /// </summary>
    /// <returns>All bindings.</returns>
    [Pure]
    public MessageBrokerChannelBinding[] GetAll()
    {
        using ( _channel.AcquireLock() )
        {
            if ( _channel.BindingsByClientId.Count == 0 )
                return Array.Empty<MessageBrokerChannelBinding>();

            var i = 0;
            var result = new MessageBrokerChannelBinding[_channel.BindingsByClientId.Count];
            foreach ( var binding in _channel.BindingsByClientId.Values )
                result[i++] = binding;

            return result;
        }
    }

    /// <summary>
    /// Attempts to return a binding by related client id.
    /// </summary>
    /// <param name="clientId">Client's unique <see cref="MessageBrokerRemoteClient.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerChannelBinding"/> instance associated with the channel and the provided <paramref name="clientId"/>
    /// or <b>null</b>, when such a binding does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerChannelBinding? TryGetByClientId(int clientId)
    {
        using ( _channel.AcquireLock() )
            return _channel.BindingsByClientId.TryGetValue( clientId, out var result ) ? result : null;
    }
}
