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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after a batch of messages has been successfully enqueued.
/// </summary>
public readonly struct MessageBrokerQueueMessagesEnqueuedEvent
{
    private MessageBrokerQueueMessagesEnqueuedEvent(MessageBrokerQueue queue, ulong traceId, int messageCount)
    {
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
        MessageCount = messageCount;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Number of messages to enqueue.
    /// </summary>
    public int MessageCount { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueMessagesEnqueuedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[MessagesEnqueued] {Source}, MessageCount = {MessageCount}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueMessagesEnqueuedEvent Create(MessageBrokerQueue queue, ulong traceId, int messageCount)
    {
        return new MessageBrokerQueueMessagesEnqueuedEvent( queue, traceId, messageCount );
    }
}
