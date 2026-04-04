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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> after message notification ACK has been sent to the server.
/// </summary>
public readonly struct MessageBrokerClientMessageAcknowledgedEvent
{
    private readonly Int31BoolPair _data1;
    private readonly int _redelivery;

    private MessageBrokerClientMessageAcknowledgedEvent(
        MessageBrokerListener listener,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        Int31BoolPair data1,
        int redelivery)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        QueueId = queueId;
        AckId = ackId;
        StreamId = streamId;
        MessageId = messageId;
        _data1 = data1;
        _redelivery = redelivery;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerListener"/> related to this event.
    /// </summary>
    public MessageBrokerListener Listener { get; }

    /// <summary>
    /// Unique id of the server-side queue that processed the message.
    /// </summary>
    public int QueueId { get; }

    /// <summary>
    /// Id of the ACK associated with the message.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Unique id of the server-side stream that handled the message.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Unique message id.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Retry attempt of the message.
    /// </summary>
    public int Retry => _data1.IntValue;

    /// <summary>
    /// Redelivery attempt of the message.
    /// </summary>
    public int Redelivery => _redelivery;

    /// <summary>
    /// Specifies whether this ACK was negative.
    /// </summary>
    public bool IsNack => _data1.BoolValue;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientMessageAcknowledgedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[MessageAcknowledged] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}', QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}, IsNack = {IsNack}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientMessageAcknowledgedEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        bool isNack)
    {
        return new MessageBrokerClientMessageAcknowledgedEvent(
            listener,
            traceId,
            queueId,
            ackId,
            streamId,
            messageId,
            new Int31BoolPair( retry, isNack ),
            redelivery );
    }
}
