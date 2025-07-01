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
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of messages stored in a <see cref="MessageBrokerStream"/>.
/// </summary>
public readonly struct MessageBrokerStreamMessageCollection
{
    private readonly MessageBrokerStream _stream;

    internal MessageBrokerStreamMessageCollection(MessageBrokerStream stream)
    {
        _stream = stream;
    }

    /// <summary>
    /// Number of stored messages.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _stream.AcquireLock() )
                return _stream.MessageStore.Count;
        }
    }

    /// <summary>
    /// Attempts to retrieve a message from the store by its store <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Store key of the message to retrieve.</param>
    /// <returns>
    /// <see cref="MessageBrokerStreamMessage"/> instance associated with the given store <paramref name="key"/>
    /// or <b>null</b> if such a message doesn't exist.
    /// </returns>
    [Pure]
    public MessageBrokerStreamMessage? TryGetByKey(int key)
    {
        using ( _stream.AcquireLock() )
        {
            if ( ! _stream.MessageStore.TryGet( key, out var message, out var refCount ) )
                return null;

            return new MessageBrokerStreamMessage(
                message.Publisher,
                MemorySize.FromBytes( message.Data.Length ),
                message.Id,
                message.PushedAt,
                key,
                refCount );
        }
    }
}
