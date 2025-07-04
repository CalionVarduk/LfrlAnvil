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
using LfrlAnvil.MessageBroker.Client.Internal;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> when attempting to process
/// received message notification from the server.
/// </summary>
public readonly struct MessageBrokerClientProcessingMessageEvent
{
    private readonly Int31BoolPair _retry;
    private readonly Int31BoolPair _redelivery;

    private MessageBrokerClientProcessingMessageEvent(
        MessageBrokerClient client,
        ulong traceId,
        int ackId,
        int senderId,
        int streamId,
        ulong messageId,
        int channelId,
        Int31BoolPair retry,
        Int31BoolPair redelivery,
        int length)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        AckId = ackId;
        SenderId = senderId;
        StreamId = streamId;
        MessageId = messageId;
        ChannelId = channelId;
        _retry = retry;
        _redelivery = redelivery;
        Length = length;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

    /// <summary>
    /// Id of the pending ACK associated with the message.
    /// </summary>
    /// <remarks>ACK is expected only when value is greater than <b>0</b>.</remarks>
    public int AckId { get; }

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
    /// Message length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Retry attempt of this message.
    /// </summary>
    public int Retry => _retry.IntValue;

    /// <summary>
    /// Specifies whether or not this is a retry of the message.
    /// </summary>
    public bool IsRetry => _retry.BoolValue;

    /// <summary>
    /// Redelivery attempt of this message.
    /// </summary>
    public int Redelivery => _redelivery.IntValue;

    /// <summary>
    /// Specifies whether or not this is a redelivery of the message.
    /// </summary>
    public bool IsRedelivery => _redelivery.BoolValue;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientProcessingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var ackId = AckId > 0 ? $", AckId = {AckId}" : string.Empty;
        var isRetry = IsRetry ? " (active)" : string.Empty;
        var isRedelivery = IsRedelivery ? " (active)" : string.Empty;
        return
            $"[ProcessingMessage] {Source}{ackId}, StreamId = {StreamId}, MessageId = {MessageId}, Retry = {Retry}{isRetry}, Redelivery = {Redelivery}{isRedelivery}, ChannelId = {ChannelId}, SenderId = {SenderId}, Length = {Length}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientProcessingMessageEvent Create(
        MessageBrokerClient client,
        ulong traceId,
        int ackId,
        int senderId,
        int streamId,
        ulong messageId,
        int channelId,
        Int31BoolPair retry,
        Int31BoolPair redelivery,
        int length)
    {
        return new MessageBrokerClientProcessingMessageEvent(
            client,
            traceId,
            ackId,
            senderId,
            streamId,
            messageId,
            channelId,
            retry,
            redelivery,
            length );
    }
}
