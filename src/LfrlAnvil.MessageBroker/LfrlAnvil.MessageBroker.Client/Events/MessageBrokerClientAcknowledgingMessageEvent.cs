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

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to sending message notification ACK to the server.
/// </summary>
public readonly struct MessageBrokerClientAcknowledgingMessageEvent
{
    private MessageBrokerClientAcknowledgingMessageEvent(
        MessageBrokerListener listener,
        ulong traceId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt,
        ulong? messageTraceId,
        MessageBrokerNegativeAck? nack,
        bool isAutomatic)
    {
        Source = MessageBrokerClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        AckId = ackId;
        StreamId = streamId;
        MessageId = messageId;
        RetryAttempt = retryAttempt;
        RedeliveryAttempt = redeliveryAttempt;
        MessageTraceId = messageTraceId;
        Nack = nack;
        IsAutomatic = isAutomatic;
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
    /// Retry attempt number of the message.
    /// </summary>
    public int RetryAttempt { get; }

    /// <summary>
    /// Redelivery attempt number of the message.
    /// </summary>
    public int RedeliveryAttempt { get; }

    /// <summary>
    /// Optional trace id of the client event that relates to receiving message notification from the server.
    /// </summary>
    public ulong? MessageTraceId { get; }

    /// <summary>
    /// Optional <see cref="MessageBrokerNegativeAck"/> instance.
    /// </summary>
    /// <remarks><b>null</b> value means that this event relates to an ACK, otherwise it relates to a negative ACK.</remarks>
    public MessageBrokerNegativeAck? Nack { get; }

    /// <summary>
    /// Specifies whether or not this ACK was initialized automatically by the client.
    /// </summary>
    /// <remarks>Applies only if <see cref="Nack"/> is not <b>null</b>.</remarks>
    public bool IsAutomatic { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientAcknowledgingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var messageTraceId = MessageTraceId is not null ? $", MessageTraceId = {MessageTraceId.Value}" : string.Empty;
        var nack = Nack is not null
            ? $", NACK = (SkipRetry = {Nack.Value.SkipRetry}{(Nack.Value.RetryDelay is not null ? $", RetryDelay = {Nack.Value.RetryDelay.Value}" : string.Empty)}, IsAutomatic = {IsAutomatic})"
            : string.Empty;

        return
            $"[AcknowledgingMessage] {Source}, Channel = [{Listener.ChannelId}] '{Listener.ChannelName}', Queue = [{Listener.QueueId}] '{Listener.QueueName}', AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, RetryAttempt = {RetryAttempt}, RedeliveryAttempt = {RedeliveryAttempt}{messageTraceId}{nack}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientAcknowledgingMessageEvent Create(
        MessageBrokerListener listener,
        ulong traceId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt,
        ulong? messageTraceId,
        MessageBrokerNegativeAck? nack,
        bool isAutomatic)
    {
        return new MessageBrokerClientAcknowledgingMessageEvent(
            listener,
            traceId,
            ackId,
            streamId,
            messageId,
            retryAttempt,
            redeliveryAttempt,
            messageTraceId,
            nack,
            isAutomatic );
    }
}
