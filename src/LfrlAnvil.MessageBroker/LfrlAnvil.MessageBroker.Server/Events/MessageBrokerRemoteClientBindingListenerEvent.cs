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
/// Represents an event emitted by <see cref="MessageBrokerRemoteClient"/> related to binding a listener.
/// </summary>
public readonly struct MessageBrokerRemoteClientBindingListenerEvent
{
    private MessageBrokerRemoteClientBindingListenerEvent(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName,
        string queueName,
        short prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        bool createChannelIfNotExists)
    {
        Source = MessageBrokerRemoteClientEventSource.Create( client, traceId );
        ChannelName = channelName;
        QueueName = queueName;
        PrefetchHint = prefetchHint;
        MaxRetries = maxRetries;
        RetryDelay = retryDelay;
        MaxRedeliveries = maxRedeliveries;
        MinAckTimeout = minAckTimeout;
        DeadLetterCapacityHint = deadLetterCapacityHint;
        MinDeadLetterRetention = minDeadLetterRetention;
        CreateChannelIfNotExists = createChannelIfNotExists;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerRemoteClientEventSource Source { get; }

    /// <summary>
    /// Channel's name.
    /// </summary>
    public string ChannelName { get; }

    /// <summary>
    /// Queue's name.
    /// </summary>
    public string QueueName { get; }

    /// <summary>
    /// Listener's prefetch hint.
    /// </summary>
    public short PrefetchHint { get; }

    /// <summary>
    /// Listener's max retries count.
    /// </summary>
    public int MaxRetries { get; }

    /// <summary>
    /// Specifies whether or not the client requested channel creation if it doesn't exist.
    /// </summary>
    public bool CreateChannelIfNotExists { get; }

    /// <summary>
    /// Listener's retry delay.
    /// </summary>
    public Duration RetryDelay { get; }

    /// <summary>
    /// Listener's min ACK timeout.
    /// </summary>
    public Duration MinAckTimeout { get; }

    /// <summary>
    /// Listener's min retention period for messages stored in the dead letter.
    /// </summary>
    public Duration MinDeadLetterRetention { get; }

    /// <summary>
    /// Listener's capacity of how many messages will be stored at most by the dead letter.
    /// </summary>
    public int DeadLetterCapacityHint { get; }

    /// <summary>
    /// Listener's max redeliveries count.
    /// </summary>
    public int MaxRedeliveries { get; }

    /// <summary>
    /// Specifies whether or not the listener has ACKs enabled.
    /// </summary>
    public bool AreAcksEnabled => MinAckTimeout > Duration.Zero;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerRemoteClientBindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var queue = QueueName.Length > 0 ? $", QueueName = '{QueueName}'" : string.Empty;
        var minAckTimeout = AreAcksEnabled ? $"MinAckTimeout = {MinAckTimeout}" : "MinAckTimeout = <disabled>";
        var deadLetter
            = $"DeadLetter = {(DeadLetterCapacityHint > 0 ? $"(CapacityHint = {DeadLetterCapacityHint}, MinRetention = {MinDeadLetterRetention})" : "<disabled>")}";

        return
            $"[BindingListener] {Source}, ChannelName = '{ChannelName}'{queue}, PrefetchHint = {PrefetchHint}, MaxRetries = {MaxRetries}, RetryDelay = {RetryDelay}, MaxRedeliveries = {MaxRedeliveries}, {minAckTimeout}, {deadLetter}, CreateChannelIfNotExists = {CreateChannelIfNotExists}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerRemoteClientBindingListenerEvent Create(
        MessageBrokerRemoteClient client,
        ulong traceId,
        string channelName,
        string queueName,
        short prefetchHint,
        int maxRetries,
        Duration retryDelay,
        int maxRedeliveries,
        Duration minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        bool createChannelIfNotExists)
    {
        return new MessageBrokerRemoteClientBindingListenerEvent(
            client,
            traceId,
            channelName,
            queueName,
            prefetchHint,
            maxRetries,
            retryDelay,
            maxRedeliveries,
            minAckTimeout,
            deadLetterCapacityHint,
            minDeadLetterRetention,
            createChannelIfNotExists );
    }
}
