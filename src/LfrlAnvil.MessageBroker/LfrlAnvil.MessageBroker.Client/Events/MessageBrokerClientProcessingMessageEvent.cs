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
/// Represents an event emitted by <see cref="MessageBrokerClient"/> when attempting to process
/// received message notification from the server.
/// </summary>
public readonly struct MessageBrokerClientProcessingMessageEvent
{
    private MessageBrokerClientProcessingMessageEvent(
        MessageBrokerClient client,
        ulong traceId,
        int senderId,
        int streamId,
        ulong messageId,
        int channelId,
        int retryAttempt,
        int redeliveryAttempt,
        int length)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        SenderId = senderId;
        StreamId = streamId;
        MessageId = messageId;
        ChannelId = channelId;
        RetryAttempt = retryAttempt;
        RedeliveryAttempt = redeliveryAttempt;
        Length = length;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Identifier of the client that published this message.
    /// </summary>
    public int SenderId { get; }

    /// <summary>
    /// Unique id of the stream through which the message has been pushed.
    /// </summary>
    public int StreamId { get; }

    /// <summary>
    /// Unique id of the message assigned by the server.
    /// </summary>
    public ulong MessageId { get; }

    /// <summary>
    /// Unique id of the channel through which the message has been sent.
    /// </summary>
    public int ChannelId { get; }

    /// <summary>
    /// Retry attempt number of this message.
    /// </summary>
    public int RetryAttempt { get; }

    /// <summary>
    /// Redelivery attempt number of this message.
    /// </summary>
    public int RedeliveryAttempt { get; }

    /// <summary>
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientProcessingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return
            $"[ProcessingMessage] {Source}, SenderId = {SenderId}, StreamId = {StreamId}, MessageId = {MessageId}, ChannelId = {ChannelId}, RetryAttempt = {RetryAttempt}, RedeliveryAttempt = {RedeliveryAttempt}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProcessingMessageEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        int senderId,
        int streamId,
        ulong messageId,
        int channelId,
        int retryAttempt,
        int redeliveryAttempt,
        int length)
    {
        return new MessageBrokerClientProcessingMessageEvent(
            client,
            traceId,
            senderId,
            streamId,
            messageId,
            channelId,
            retryAttempt,
            redeliveryAttempt,
            length );
    }
}
