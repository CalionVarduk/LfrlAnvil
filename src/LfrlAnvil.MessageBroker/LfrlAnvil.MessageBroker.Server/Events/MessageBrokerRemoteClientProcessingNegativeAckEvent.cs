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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> when attempting to handle a negative ACK sent by the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientProcessingNegativeAckEvent
{
    private MessageBrokerRemoteClientProcessingNegativeAckEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt,
        bool noRetry,
        Duration? explicitDelay)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        QueueId = queueId;
        AckId = ackId;
        StreamId = streamId;
        MessageId = messageId;
        RetryAttempt = retryAttempt;
        RedeliveryAttempt = redeliveryAttempt;
        NoRetry = noRetry;
        ExplicitDelay = explicitDelay;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Unique id of the queue that handles the message.
    /// </summary>
    public int QueueId { get; }

    /// <summary>
    /// Id of the ACK associated with the message.
    /// </summary>
    public int AckId { get; }

    /// <summary>
    /// Unique id of the stream that owns the message.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Unique id of the message.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Retry attempt of the message.
    /// </summary>
    public int RetryAttempt { get; }

    /// <summary>
    /// Redelivery attempt of the message.
    /// </summary>
    public int RedeliveryAttempt { get; }

    /// <summary>
    /// Specifies whether or not the client requested to not send retries for the message.
    /// </summary>
    public bool NoRetry { get; }

    /// <summary>
    /// Specifies explicit retry delay requested by the client.
    /// </summary>
    public Duration? ExplicitDelay { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientProcessingNegativeAckEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var explicitDelay = ExplicitDelay is not null ? $", ExplicitDelay = {ExplicitDelay.Value}" : string.Empty;
        return
            $"[ProcessingNegativeAck] {Source}, QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, RetryAttempt = {RetryAttempt}, RedeliveryAttempt = {RedeliveryAttempt}, NoRetry = {NoRetry}{explicitDelay}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientProcessingNegativeAckEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt,
        bool noRetry,
        Duration? explicitDelay)
    {
        return new MessageBrokerRemoteClientProcessingNegativeAckEvent(
            client,
            traceId,
            queueId,
            ackId,
            streamId,
            messageId,
            retryAttempt,
            redeliveryAttempt,
            noRetry,
            explicitDelay );
    }
}
