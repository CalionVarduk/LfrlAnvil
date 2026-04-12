// Copyright 2026 Łukasz Furlepa
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
/// Represents a collection of <see cref="MessageBrokerQueueListenerBinding"/> instances attached to a single channel listener binding,
/// identified by queue ids.
/// </summary>
public readonly struct MessageBrokerChannelListenerBindingQueueBindingCollection
{
    private readonly MessageBrokerChannelListenerBinding _listener;

    internal MessageBrokerChannelListenerBindingQueueBindingCollection(MessageBrokerChannelListenerBinding listener)
    {
        _listener = listener;
    }

    /// <summary>
    /// Specifies the number of queue bindings.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _listener.AcquireLock() )
                return _listener.QueueBindingCollection.Count;
        }
    }

    /// <summary>
    /// Specifies the primary queue binding.
    /// </summary>
    public MessageBrokerQueueListenerBinding Primary
    {
        get
        {
            using ( _listener.AcquireLock() )
                return _listener.QueueBindingCollection.Primary;
        }
    }

    /// <summary>
    /// Returns all queue bindings.
    /// </summary>
    /// <returns>All queue bindings.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerQueueListenerBinding> GetAll()
    {
        using ( _listener.AcquireLock() )
            return _listener.QueueBindingCollection.GetAllUnsafe();
    }

    /// <summary>
    /// Attempts to return a queue binding by related queue id.
    /// </summary>
    /// <param name="queueId">Queue's unique <see cref="MessageBrokerQueue.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueueListenerBinding"/> instance associated with the channel listener
    /// and the provided <paramref name="queueId"/> or <b>null</b>, when such a queue binding does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueueListenerBinding? TryGetByQueueId(int queueId)
    {
        using ( _listener.AcquireLock() )
        {
            if ( _listener.QueueBindingCollection.Primary.Queue.Id == queueId )
                return _listener.QueueBindingCollection.Primary;

            foreach ( var b in _listener.QueueBindingCollection.Secondary )
            {
                if ( b.Queue.Id == queueId )
                    return b;
            }

            return null;
        }
    }
}
