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
/// Collection of scheduled message retries stored in a <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueueMessageRetriesCollection
{
    private readonly MessageBrokerQueue _queue;

    internal MessageBrokerQueueMessageRetriesCollection(MessageBrokerQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// Number of scheduled message retries.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _queue.AcquireLock() )
                return _queue.MessageStore.Retries.Count;
        }
    }

    /// <summary>
    /// Attempts to retrieve the next scheduled message retry in the queue.
    /// </summary>
    /// <returns>
    /// Next <see cref="MessageBrokerQueueMessageRetry"/> or <b>null</b> if scheduled message retries queue is empty.
    /// </returns>
    [Pure]
    public MessageBrokerQueueMessageRetry? TryGetNext()
    {
        using ( _queue.AcquireLock() )
        {
            if ( _queue.MessageStore.Retries.IsEmpty )
                return null;

            ref var first = ref _queue.MessageStore.Retries.First();
            return new MessageBrokerQueueMessageRetry(
                first.Message.Publisher,
                first.Message.Listener,
                first.Message.StoreKey,
                first.RetryAttempt,
                first.RedeliveryAttempt,
                first.SendAt );
        }
    }
}
