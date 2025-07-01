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
using LfrlAnvil.MessageBroker.Server.Internal;

namespace LfrlAnvil.MessageBroker.Server.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> after message notification
/// has been successfully sent to the client.
/// </summary>
public readonly struct MessageBrokerRemoteClientMessageProcessedEvent
{
    private readonly ResendIndex _retryAttempt;
    private readonly ResendIndex _redeliveryAttempt;

    private MessageBrokerRemoteClientMessageProcessedEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int ackId,
        ResendIndex retryAttempt,
        ResendIndex redeliveryAttempt)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( listener.Client, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageId = messageId;
        AckId = ackId;
        _retryAttempt = retryAttempt;
        _redeliveryAttempt = redeliveryAttempt;
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
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that sent the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

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
    /// Retry attempt number of this message.
    /// </summary>
    public int RetryAttempt => _retryAttempt.Value;

    /// <summary>
    /// Specifies whether or not this message is a retry.
    /// </summary>
    public bool IsRetry => _retryAttempt.IsActive;

    /// <summary>
    /// Redelivery attempt number of this message.
    /// </summary>
    public int RedeliveryAttempt => _redeliveryAttempt.Value;

    /// <summary>
    /// Specifies whether or not this message is a redelivery.
    /// </summary>
    public bool IsRedelivery => _redeliveryAttempt.IsActive;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientMessageProcessedEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var ackId = AckId > 0 ? $", AckId = {AckId}" : string.Empty;
        var isRetry = IsRetry ? " (active)" : string.Empty;
        var isRedelivery = IsRedelivery ? " (active)" : string.Empty;
        return
            $"[MessageProcessed] {Source}, Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', Queue = [{Listener.Queue.Id}] '{Listener.Queue.Name}'{ackId}, MessageId = {MessageId}, RetryAttempt = {RetryAttempt}{isRetry}, RedeliveryAttempt = {RedeliveryAttempt}{isRedelivery}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientMessageProcessedEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        ulong messageId,
        int ackId,
        ResendIndex retryAttempt,
        ResendIndex redeliveryAttempt)
    {
        return new MessageBrokerRemoteClientMessageProcessedEvent(
            listener,
            traceId,
            publisher,
            messageId,
            ackId,
            retryAttempt,
            redeliveryAttempt );
    }
}
