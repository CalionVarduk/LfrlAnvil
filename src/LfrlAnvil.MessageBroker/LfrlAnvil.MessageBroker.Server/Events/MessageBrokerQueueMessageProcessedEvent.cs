// Copyright 2025-2026 Łukasz Furlepa
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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after an enqueued message has been processed.
/// </summary>
public readonly struct MessageBrokerQueueMessageProcessedEvent
{
    private MessageBrokerQueueMessageProcessedEvent(
        MessageBrokerQueueListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        ulong messageId,
        int ackId,
        bool messageRemoved,
        Timestamp ackExpiresAt)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
        AckId = ackId;
        MessageRemoved = messageRemoved;
        AckExpiresAt = ackExpiresAt;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerQueueListenerBinding"/> that received the message.
    /// </summary>
    public MessageBrokerQueueListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed the message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Id of the pending ACK associated with the message.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Specifies whether the data of the message has been removed from the stream's message store
    /// due to no longer being referenced.
    /// </summary>
    public bool MessageRemoved { get; }

    /// <summary>
    /// Moment of pending ACK expiration.
    /// </summary>
    public Timestamp AckExpiresAt { get; }

    /// <summary>
    /// Specifies whether the message is from dead letter.
    /// </summary>
    public bool IsFromDeadLetter => AckId < 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var ack = AckId switch
        {
            > 0 => $", Ack = (Id = {AckId}, ExpiresAt = {AckExpiresAt})",
            -1 => ", Ack = <dead-letter>",
            _ => string.Empty
        };

        var messageRemoved = AckId <= 0 ? $", MessageRemoved = {MessageRemoved}" : string.Empty;
        return
            $"[MessageProcessed] {Source}, Sender = [{Publisher.ClientId}] '{Publisher.ClientName}', Channel = [{Listener.Owner.Channel.Id}] '{Listener.Owner.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', MessageId = {MessageId}{ack}{messageRemoved}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueMessageProcessedEvent Create(
        MessageBrokerQueueListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        ulong messageId,
        int ackId,
        bool messageRemoved,
        Timestamp ackExpiresAt)
    {
        return new MessageBrokerQueueMessageProcessedEvent(
            listener,
            traceId,
            publisher,
            messageId,
            ackId,
            messageRemoved,
            ackExpiresAt );
    }
}
