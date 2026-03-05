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
using LfrlAnvil.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> after message notification
/// has been successfully sent to the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientMessageProcessedEvent
{
    private readonly Int31BoolPair _retry;
    private readonly Int31BoolPair _redelivery;

    private MessageBrokerRemoteClientMessageProcessedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        ulong messageId,
        int ackId,
        Int31BoolPair retry,
        Int31BoolPair redelivery)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
        AckId = ackId;
        _retry = retry;
        _redelivery = redelivery;
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
    /// <see cref="IMessageBrokerMessagePublisher"/> that sent the message.
    /// </summary>
    public IMessageBrokerMessagePublisher Publisher { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Id of the pending ACK associated with the message.
    /// </summary>
    /// <remarks>ACK is expected only when value is greater than <b>0</b>.</remarks>
    public int AckId { get; }

    /// <summary>
    /// Retry attempt of this message.
    /// </summary>
    public int Retry => _retry.IntValue;

    /// <summary>
    /// Specifies whether this message is a retry.
    /// </summary>
    public bool IsRetry => _retry.BoolValue;

    /// <summary>
    /// Redelivery attempt of this message.
    /// </summary>
    public int Redelivery => _redelivery.IntValue;

    /// <summary>
    /// Specifies whether this message is a redelivery.
    /// </summary>
    public bool IsRedelivery => _redelivery.BoolValue;

    /// <summary>
    /// Specifies whether this message is from dead letter.
    /// </summary>
    public bool IsFromDeadLetter => AckId < 0;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var ackId = AckId switch
        {
            > 0 => $", AckId = {AckId}",
            -1 => ", AckId = <dead-letter>",
            _ => string.Empty
        };

        var isRetry = IsRetry ? " (active)" : string.Empty;
        var isRedelivery = IsRedelivery ? " (active)" : string.Empty;
        return
            $"[MessageProcessed] {Source}, Sender = [{Publisher.ClientId}] '{Publisher.ClientName}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', Queue = [{Listener.Queue.Id}] '{Listener.Queue.Name}'{ackId}, MessageId = {MessageId}, Retry = {Retry}{isRetry}, Redelivery = {Redelivery}{isRedelivery}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientMessageProcessedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        IMessageBrokerMessagePublisher publisher,
        ulong messageId,
        int ackId,
        Int31BoolPair retry,
        Int31BoolPair redelivery)
    {
        return new MessageBrokerRemoteClientMessageProcessedEvent( listener, traceId, publisher, messageId, ackId, retry, redelivery );
    }
}
