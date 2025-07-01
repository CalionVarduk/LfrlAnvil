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
/// Represents an event emitted by <see cref="MessageBrokerQueue"/> when starting to process an enqueued message.
/// </summary>
public readonly struct MessageBrokerQueueProcessingMessageEvent
{
    private readonly ResendIndex _retryAttempt;
    private readonly ResendIndex _redeliveryAttempt;

    private MessageBrokerQueueProcessingMessageEvent(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        int messageStoreKey,
        ResendIndex retryAttempt,
        ResendIndex redeliveryAttempt)
    {
        Source = MessageBrokerQueueEventSource.Create( listener.Queue, traceId );
        Listener = listener;
        Publisher = publisher;
        MessageStoreKey = messageStoreKey;
        _retryAttempt = retryAttempt;
        _redeliveryAttempt = redeliveryAttempt;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerQueueEventSource Source { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelListenerBinding"/> that received the message.
    /// </summary>
    public MessageBrokerChannelListenerBinding Listener { get; }

    /// <summary>
    /// <see cref="MessageBrokerChannelPublisherBinding"/> that pushed the message.
    /// </summary>
    public MessageBrokerChannelPublisherBinding Publisher { get; }

    /// <summary>
    /// Key of the stream's message store entry associated with the message.
    /// </summary>
    public int MessageStoreKey { get; }

    /// <summary>
    /// Specifies whether or not this message is a retry.
    /// </summary>
    public bool IsRetry => _retryAttempt.IsActive;

    /// <summary>
    /// Retry attempt number of this message.
    /// </summary>
    public int RetryAttempt => _retryAttempt.Value;

    /// <summary>
    /// Specifies whether or not this message is a redelivery.
    /// </summary>
    public bool IsRedelivery => _redeliveryAttempt.IsActive;

    /// <summary>
    /// Redelivery attempt number of this message.
    /// </summary>
    public int RedeliveryAttempt => _redeliveryAttempt.Value;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerQueueProcessingMessageEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var isRetry = IsRetry ? " (active)" : string.Empty;
        var isRedelivery = IsRedelivery ? " (active)" : string.Empty;
        return
            $"[ProcessingMessage] {Source}, Sender = [{Publisher.Client.Id}] '{Publisher.Client.Name}', Channel = [{Listener.Channel.Id}] '{Listener.Channel.Name}', Stream = [{Publisher.Stream.Id}] '{Publisher.Stream.Name}', MessageStoreKey = {MessageStoreKey}, RetryAttempt = {RetryAttempt}{isRetry}, RedeliveryAttempt = {RedeliveryAttempt}{isRedelivery}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerQueueProcessingMessageEvent Create(
        MessageBrokerChannelListenerBinding listener,
        ulong traceId,
        MessageBrokerChannelPublisherBinding publisher,
        int messageStoreKey,
        ResendIndex retryAttempt,
        ResendIndex redeliveryAttempt)
    {
        return new MessageBrokerQueueProcessingMessageEvent(
            listener,
            traceId,
            publisher,
            messageStoreKey,
            retryAttempt,
            redeliveryAttempt );
    }
}
