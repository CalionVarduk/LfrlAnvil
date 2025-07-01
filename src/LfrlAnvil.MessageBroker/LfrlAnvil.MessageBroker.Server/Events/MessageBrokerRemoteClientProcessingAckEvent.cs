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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> when attempting to handle an ACK sent by the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientProcessingAckEvent
{
    private MessageBrokerRemoteClientProcessingAckEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        QueueId = queueId;
        AckId = ackId;
        StreamId = streamId;
        MessageId = messageId;
        RetryAttempt = retryAttempt;
        RedeliveryAttempt = redeliveryAttempt;
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
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientProcessingAckEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[ProcessingAck] {Source}, QueueId = {QueueId}, AckId = {AckId}, StreamId = {StreamId}, MessageId = {MessageId}, RetryAttempt = {RetryAttempt}, RedeliveryAttempt = {RedeliveryAttempt}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientProcessingAckEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        int queueId,
        int ackId,
        int streamId,
        ulong messageId,
        int retryAttempt,
        int redeliveryAttempt)
    {
        return new MessageBrokerRemoteClientProcessingAckEvent(
            client,
            traceId,
            queueId,
            ackId,
            streamId,
            messageId,
            retryAttempt,
            redeliveryAttempt );
    }
}
