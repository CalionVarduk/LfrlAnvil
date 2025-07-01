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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> after an enqueued message has been processed.
/// </summary>
public readonly struct MessageBrokerQueueMessageProcessedEvent
{
    private MessageBrokerQueueMessageProcessedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int ackId,
        Timestamp ackExpiresAt,
        int length,
        bool messageDataRemoved)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
        AckId = ackId;
        AckExpiresAt = ackExpiresAt;
        Length = length;
        MessageDataRemoved = messageDataRemoved;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that received the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Id of the pending ACK associated with the message.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Moment of pending ACK expiration.
    /// </summary>
    public Timestamp AckExpiresAt { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Specifies whether or not the data of the message has been removed from the stream's message store
    /// due to no longer being referenced.
    /// </summary>
    public bool MessageDataRemoved { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var ack = AckId > 0 ? $", Ack = (Id = {AckId}, ExpiresAt = {AckExpiresAt})" : string.Empty;
        var messageDataRemoved = AckId == 0 ? $", MessageDataRemoved = {MessageDataRemoved}" : string.Empty;
        return
            $"[MessageProcessed] {Source}, Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', MessageId = {MessageId}{ack}{messageDataRemoved}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueMessageProcessedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int ackId,
        Timestamp ackExpiresAt,
        int length,
        bool messageDataRemoved)
    {
        return new MessageBrokerQueueMessageProcessedEvent(
            listener,
            traceId,
            publisher,
            messageId,
            ackId,
            ackExpiresAt,
            length,
            messageDataRemoved );
    }
}
