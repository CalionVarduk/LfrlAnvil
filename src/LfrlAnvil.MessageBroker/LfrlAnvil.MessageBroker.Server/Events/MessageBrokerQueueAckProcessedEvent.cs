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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after a message notification ACK has been processed.
/// </summary>
public readonly struct MessageBrokerQueueAckProcessedEvent
{
    private MessageBrokerQueueAckProcessedEvent(MessageBrokerQueue queue, ulong traceId, int ackId, bool messageDataRemoved)
    {
        Source = MessageBrokerQueueEventSource.Create( queue, traceId );
        AckId = ackId;
        MessageDataRemoved = messageDataRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// Id of the processed ACK.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Specifies whether or not the data of the message has been removed from the stream's message store
    /// due to no longer being referenced.
    /// </summary>
    public bool MessageDataRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueAckProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var messageDataRemoved = AckId == 0 ? $", MessageDataRemoved = {MessageDataRemoved}" : string.Empty;
        return $"[AckProcessed] {Source}, AckId = {AckId}{messageDataRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueAckProcessedEvent Create(MessageBrokerQueue queue, ulong traceId, int ackId, bool messageDataRemoved)
    {
        return new MessageBrokerQueueAckProcessedEvent( queue, traceId, ackId, messageDataRemoved );
    }
}
