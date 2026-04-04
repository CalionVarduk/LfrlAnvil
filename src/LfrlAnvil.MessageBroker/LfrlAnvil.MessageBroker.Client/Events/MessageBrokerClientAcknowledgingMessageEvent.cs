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
using LfrlAnvil.Internal;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to sending message notification ACK to the server.
/// </summary>
public readonly struct MessageBrokerClientAcknowledgingMessageEvent
{
    private const ulong NackExistsMask = 1UL << 63;
    private const ulong NackSkipRetryMask = 1UL << 62;
    private const ulong NackSkipDeadLetterMask = 1UL << 61;
    private const ulong NackDelayExistsMask = 1UL << 60;
    private const ulong NackDelayMask = NackDelayExistsMask - 1;

    private readonly Int31BoolPair _data1;
    private readonly Int31BoolPair _data2;
    private readonly ulong _messageTraceId;
    private readonly ulong _nackData;

    private MessageBrokerClientAcknowledgingMessageEvent(
        MessageBrokerListener listener,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        Int31BoolPair data1,
        Int31BoolPair data2,
        ulong messageTraceId,
        ulong nackData)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        QueueId = queueId;
        AckId = ackId;
        StreamId = streamId;
        MessageId = messageId;
        _data1 = data1;
        _data2 = data2;
        _messageTraceId = messageTraceId;
        _nackData = nackData;
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
    /// Id of the pending ACK associated with the message.
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
    /// Optional <see cref="MessageBrokerNegativeAck"/> instance.
    /// </summary>
    /// <remarks><b>null</b> value means that this event relates to an ACK, otherwise it relates to a negative ACK.</remarks>
    public MessageBrokerNegativeAck? Nack => (_nackData & NackExistsMask) != 0
        ? new MessageBrokerNegativeAck(
            (_nackData & NackSkipRetryMask) != 0,
            (_nackData & NackSkipDeadLetterMask) != 0,
            (_nackData & NackDelayExistsMask) != 0 ? Duration.FromTicks( unchecked( ( long )(_nackData & NackDelayMask) ) ) : null )
        : null;

    /// <summary>
    /// Retry attempt of the message.
    /// </summary>
    public int Retry => _data1.IntValue;

    /// <summary>
    /// Redelivery attempt of the message.
    /// </summary>
    public int Redelivery => _data2.IntValue;

    /// <summary>
    /// Optional trace id of the client event that relates to receiving message notification from the server.
    /// </summary>
    public ulong? MessageTraceId => _data2.BoolValue ? _messageTraceId : null;

    /// <summary>
    /// Specifies whether this ACK was initialized automatically by the client.
    /// </summary>
    /// <remarks>Applies only if <see cref="Nack"/> is not <b>null</b>.</remarks>
    public bool IsAutomatic => _data1.BoolValue;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientAcknowledgingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var messageTraceIdValue = MessageTraceId;
        var nackValue = Nack;
        var messageTraceId = messageTraceIdValue is not null ? $", MessageTraceId = {messageTraceIdValue.Value}" : string.Empty;
        var nack = nackValue is not null
            ? $", NACK = (SkipRetry = {nackValue.Value.SkipRetry}, SkipDeadLetter = {nackValue.Value.SkipDeadLetter}{(nackValue.Value.RetryDelay is not null ? $", RetryDelay = {nackValue.Value.RetryDelay.Value}" : string.Empty)}, IsAutomatic = {IsAutomatic})"
            : string.Empty;

        return
            $"[AcknowledgingMessage] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}', QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}, Redelivery = {Redelivery}{messageTraceId}{nack}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientAcknowledgingMessageEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retry,
        int redelivery,
        ulong? messageTraceId,
        MessageBrokerNegativeAck? nack,
        bool isAutomatic)
    {
        var nackData = 0UL;
        if ( nack is not null )
        {
            nackData |= NackExistsMask;
            nackData |= nack.Value.SkipRetry ? NackSkipRetryMask : 0;
            nackData |= nack.Value.SkipDeadLetter ? NackSkipDeadLetterMask : 0;
            nackData |= nack.Value.RetryDelay is not null
                ? unchecked( ( ulong )nack.Value.RetryDelay.Value.Ticks | NackDelayExistsMask )
                : 0;
        }

        return new MessageBrokerClientAcknowledgingMessageEvent(
            listener,
            traceId,
            queueId,
            ackId,
            streamId,
            messageId,
            new Int31BoolPair( retry, isAutomatic ),
            new Int31BoolPair( redelivery, messageTraceId is not null ),
            messageTraceId ?? 0,
            nackData );
    }
}
