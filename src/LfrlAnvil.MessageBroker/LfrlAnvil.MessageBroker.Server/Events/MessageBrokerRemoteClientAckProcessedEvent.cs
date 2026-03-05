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

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> after an ACK sent by the client has been processed successfully.
/// </summary>
public readonly struct MessageBrokerRemoteClientAckProcessedEvent
{
    private MessageBrokerRemoteClientAckProcessedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        int ackId,
        ulong messageId,
        int retry,
        int redelivery,
        bool isNack)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        Publisher = publisher;
        AckId = ackId;
        MessageId = messageId;
        Retry = retry;
        Redelivery = redelivery;
        IsNack = isNack;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> that received the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="IMessageBrokerMessagePublisher"/> that pushed the message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Id of the ACK associated with the message.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Retry attempt of the message.
    /// </summary>
    public int Retry { get; }

    /// <summary>
    /// Redelivery attempt of the message.
    /// </summary>
    public int Redelivery { get; }

    /// <summary>
    /// Specifies whether the ACK is negative.
    /// </summary>
    public bool IsNack { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientAckProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[AckProcessed] {Source}, Sender = [{Publisher.ClientId}] '{Publisher.ClientName}', Channel = [{Publisher.Channel.Id}] '{Publisher.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', Queue = [{Listener.Queue.Id}] '{Listener.Queue.Name}', AckId = {AckId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}, IsNack = {IsNack}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientAckProcessedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        int ackId,
        ulong messageId,
        int retry,
        int redelivery,
        bool isNack)
    {
        return new MessageBrokerRemoteClientAckProcessedEvent( listener, traceId, publisher, ackId, messageId, retry, redelivery, isNack );
    }
}
