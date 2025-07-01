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
/// Represents a collection of pending messages stored in a <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueuePendingMessageCollection
{
    private readonly MessageBrokerQueue _queue;

    internal MessageBrokerQueuePendingMessageCollection(MessageBrokerQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// Number of stored pending messages.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _queue.AcquireLock() )
                return _queue.MessageStore.Pending.Count;
        }
    }

    /// <summary>
    /// Attempts to retrieve a pending message from the store by its queue <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Queue index of the pending message to retrieve.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueuePendingMessage"/> instance associated with the given queue <paramref name="index"/>
    /// or <b>null</b> if such a message doesn't exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueuePendingMessage? TryPeekAt(int index)
    {
        using ( _queue.AcquireLock() )
        {
            if ( index < 0 || index >= _queue.MessageStore.Pending.Count )
                return null;

            ref var entry = ref _queue.MessageStore.Pending[index];
            return new MessageBrokerQueuePendingMessage( entry.Publisher, entry.Listener, entry.StoreKey );
        }
    }
}
