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
/// Represents a collection of <see cref="MessageBrokerSubscription"/> instances attached to a single queue, identified by channel ids.
/// </summary>
public readonly struct MessageBrokerQueueSubscriptionCollection
{
    private readonly MessageBrokerQueue _queue;

    internal MessageBrokerQueueSubscriptionCollection(MessageBrokerQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// Specifies the number of subscriptions.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _queue.AcquireLock() )
                return _queue.SubscriptionsByChannelId.Count;
        }
    }

    /// <summary>
    /// Returns all subscriptions.
    /// </summary>
    /// <returns>All subscriptions.</returns>
    [Pure]
    public ReadOnlyArray<MessageBrokerSubscription> GetAll()
    {
        using ( _queue.AcquireLock() )
            return _queue.SubscriptionsByChannelId.GetAll();
    }

    /// <summary>
    /// Attempts to return a subscription by related channel id.
    /// </summary>
    /// <param name="channelId">Channel's unique <see cref="MessageBrokerChannel.Id"/>.</param>
    /// <returns>
    /// <see cref="MessageBrokerSubscription"/> instance associated with the queue and the provided <paramref name="channelId"/>
    /// or <b>null</b>, when such a subscription does not exist.
    /// </returns>
    [Pure]
    public MessageBrokerSubscription? TryGetByChannelId(int channelId)
    {
        using ( _queue.AcquireLock() )
            return _queue.SubscriptionsByChannelId.TryGet( channelId, out var result ) ? result : null;
    }
}
