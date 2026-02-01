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

using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Client.Events;

/// <summary>
/// Represents an event emitted by <see cref="MessageBrokerClient"/> related to binding a listener.
/// </summary>
public readonly struct MessageBrokerClientBindingListenerEvent
{
    private MessageBrokerClientBindingListenerEvent(
        MessageBrokerClient client,
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
        string? filterExpression,
        bool createChannelIfNotExists,
        bool isEphemeral)
    {
        Source = MessageBrokerClientEventSource.Create( client, traceId );
        ChannelName = channelName;
        QueueName = queueName;
        PrefetchHint = prefetchHint;
        MaxRetries = maxRetries;
        RetryDelay = retryDelay;
        MaxRedeliveries = maxRedeliveries;
        MinAckTimeout = minAckTimeout;
        DeadLetterCapacityHint = deadLetterCapacityHint;
        MinDeadLetterRetention = minDeadLetterRetention;
        FilterExpression = filterExpression;
        CreateChannelIfNotExists = createChannelIfNotExists;
        IsEphemeral = isEphemeral;
    }

    /// <summary>
    /// Event source.
    /// </summary>
    public MessageBrokerClientEventSource Source { get; }

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
    /// Specifies whether or not the server should create the channel if it does not exist yet.
    /// </summary>
    public bool CreateChannelIfNotExists { get; }

    /// <summary>
    /// Specifies whether or not the listener will be ephemeral.
    /// </summary>
    public bool IsEphemeral { get; }

    /// <summary>
    /// Listener's retry delay.
    /// </summary>
    public Duration RetryDelay { get; }

    /// <summary>
    /// Listener's min ACK timeout.
    /// </summary>
    public Duration MinAckTimeout { get; }

    /// <summary>
    /// Retention period for messages stored in the dead letter.
    /// </summary>
    public Duration MinDeadLetterRetention { get; }

    /// <summary>
    /// How many messages will be stored at most by the dead letter.
    /// </summary>
    public int DeadLetterCapacityHint { get; }

    /// <summary>
    /// Listener's max redeliveries count.
    /// </summary>
    public int MaxRedeliveries { get; }

    /// <summary>
    /// Listener's server-side message filter expression.
    /// </summary>
    public string? FilterExpression { get; }

    /// <summary>
    /// Specifies whether or not the listener has ACKs enabled.
    /// </summary>
    public bool AreAcksEnabled => MinAckTimeout > Duration.Zero;

    /// <summary>
    /// Returns a string representation of this <see cref="MessageBrokerClientBindingListenerEvent"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var retries = $"MaxRetries = {MaxRetries}{(MaxRetries > 0 ? $", RetryDelay = {RetryDelay}" : string.Empty)}";
        var minAckTimeout = AreAcksEnabled ? $"MinAckTimeout = {MinAckTimeout}" : "MinAckTimeout = <disabled>";
        var deadLetter
            = $"DeadLetter = {(DeadLetterCapacityHint > 0 ? $"(CapacityHint = {DeadLetterCapacityHint}, MinRetention = {MinDeadLetterRetention})" : "<disabled>")}";

        var filterExpression = FilterExpression is not null ? $", FilterExpression:{Environment.NewLine}{FilterExpression}" : string.Empty;

        return
            $"[BindingListener] {Source}, ChannelName = '{ChannelName}', QueueName = '{QueueName}', PrefetchHint = {PrefetchHint}, {retries}, MaxRedeliveries = {MaxRedeliveries}, {minAckTimeout}, {deadLetter}, IsEphemeral = {IsEphemeral}, CreateChannelIfNotExists = {CreateChannelIfNotExists}{filterExpression}";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static MessageBrokerClientBindingListenerEvent Create(
        MessageBrokerClient client,
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
        string? filterExpression,
        bool createChannelIfNotExists,
        bool isEphemeral)
    {
        return new MessageBrokerClientBindingListenerEvent(
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
            filterExpression,
            createChannelIfNotExists,
            isEphemeral );
    }
}
