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
using System.Runtime.CompilerServices;
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server;

/// <summary>
/// Represents a collection of unacked messages stored in a <see cref="MessageBrokerQueue"/>.
/// </summary>
public readonly struct MessageBrokerQueueUnackedMessageCollection
{
    private readonly MessageBrokerQueue _queue;

    internal MessageBrokerQueueUnackedMessageCollection(MessageBrokerQueue queue)
    {
        _queue = queue;
    }

    /// <summary>
    /// Number of stored unacked messages.
    /// </summary>
    public int Count
    {
        get
        {
            using ( _queue.AcquireLock() )
                return _queue.MessageStore.Unacked.Count;
        }
    }

    /// <summary>
    /// Attempts to retrieve the first unacked message in the queue.
    /// </summary>
    /// <returns>
    /// First <see cref="MessageBrokerQueueUnackedMessage"/> or <b>null</b> if unacked message queue is empty.
    /// </returns>
    [Pure]
    public MessageBrokerQueueUnackedMessage? TryGetFirst()
    {
        using ( _queue.AcquireLock() )
            return FromNode( _queue.MessageStore.Unacked.First );
    }

    /// <summary>
    /// Attempts to retrieve the last unacked message in the queue.
    /// </summary>
    /// <returns>
    /// Last <see cref="MessageBrokerQueueUnackedMessage"/> or <b>null</b> if unacked message queue is empty.
    /// </returns>
    [Pure]
    public MessageBrokerQueueUnackedMessage? TryGetLast()
    {
        using ( _queue.AcquireLock() )
            return FromNode( _queue.MessageStore.Unacked.Last );
    }

    /// <summary>
    /// Attempts to retrieve an unacked message from the store by its <paramref name="ackId"/>.
    /// </summary>
    /// <param name="ackId">Ack id of the unacked message to retrieve.</param>
    /// <returns>
    /// <see cref="MessageBrokerQueueUnackedMessage"/> instance associated with the given <paramref name="ackId"/>
    /// or <b>null</b> if such a message doesn't exist.
    /// </returns>
    [Pure]
    public MessageBrokerQueueUnackedMessage? TryGetByAckId(int ackId)
    {
        using ( _queue.AcquireLock() )
            return FromNode( _queue.MessageStore.Unacked.GetNode( ackId - 1 ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static MessageBrokerQueueUnackedMessage? FromNode(LinkedListSlimNode<QueueMessageStore.UnackedEntry>? node)
    {
        if ( node is null )
            return null;

        ref var entry = ref node.Value.Value;
        return new MessageBrokerQueueUnackedMessage(
            entry.Message.Publisher,
            entry.Message.Listener,
            entry.Message.StoreKey,
            entry.MessageId,
            entry.Retry,
            entry.Redelivery,
            entry.ExpiresAt );
    }
}
