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
using LfrlAnvil.Chrono;

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents available <see cref="MessageBrokerListener"/> options during its binding.
/// </summary>
public readonly struct MessageBrokerListenerOptions
{
    /// <summary>
    /// Default <see cref="PrefetchHint"/>. Equal to <b>1</b>.
    /// </summary>
    public const short DefaultPrefetchHint = 1;

    /// <summary>
    /// Default <see cref="RetryDelay"/>. Equal to <b>30 seconds</b>.
    /// </summary>
    public static Duration DefaultRetryDelay => Duration.FromSeconds( 30 );

    /// <summary>
    /// Default <see cref="MinAckTimeout"/>. Equal to <b>10 minutes</b>.
    /// </summary>
    public static Duration DefaultMinAckTimeout => Duration.FromMinutes( 10 );

    /// <summary>
    /// Represents default options, with default prefetch hint, disabled retries and redeliveries and dead letter,
    /// lack of filter expression, and enabled ACKs.
    /// </summary>
    public static MessageBrokerListenerOptions Default =>
        new MessageBrokerListenerOptions( null, 0, null, 0, null, 0, Duration.Zero, null, false );

    private readonly short? _prefetchHint;
    private readonly Duration? _retryDelay;
    private readonly Duration? _minAckTimeout;
    private readonly Duration _minDeadLetterRetention;
    private readonly bool _acksDisabled;

    private MessageBrokerListenerOptions(
        short? prefetchHint,
        int maxRetries,
        Duration? retryDelay,
        int maxRedeliveries,
        Duration? minAckTimeout,
        int deadLetterCapacityHint,
        Duration minDeadLetterRetention,
        string? filterExpression,
        bool acksDisabled)
    {
        _prefetchHint = prefetchHint;
        _retryDelay = retryDelay;
        _minAckTimeout = minAckTimeout;
        _minDeadLetterRetention = minDeadLetterRetention;
        _acksDisabled = acksDisabled;
        MaxRetries = maxRetries;
        MaxRedeliveries = maxRedeliveries;
        DeadLetterCapacityHint = deadLetterCapacityHint;
        FilterExpression = filterExpression;
    }

    /// <summary>
    /// Specifies how many times the server will attempt to automatically send a message notification retry
    /// when the client responds with a negative ACK, before giving up.
    /// </summary>
    /// <remarks>Retries will be disabled when value is equal <b>0</b>.</remarks>
    public int MaxRetries { get; }

    /// <summary>
    /// Specifies how many times the server will attempt to automatically send a message notification redelivery
    /// when the client fails to respond with either an ACK or a negative ACK in time (see <see cref="MinAckTimeout"/>), before giving up.
    /// </summary>
    /// <remarks>Redelivery will be disabled when value is equal <b>0</b>.</remarks>
    public int MaxRedeliveries { get; }

    /// <summary>
    /// Specifies how many messages will be stored at most by the dead letter.
    /// </summary>
    /// <remarks>
    /// This is a min value. Actual value is dependant on all listeners attached to the queue and the state of the queue's dead letter.
    /// </remarks>
    public int DeadLetterCapacityHint { get; }

    /// <summary>
    /// Specifies server-side message filter expression.
    /// </summary>
    public string? FilterExpression { get; }

    /// <summary>
    /// Specifies the retention period for messages stored in the dead letter.
    /// </summary>
    /// <remarks>
    /// This is a min value. Actual value is dependant on all listeners attached to the queue and the state of the queue's dead letter.
    /// </remarks>
    public Duration MinDeadLetterRetention => DeadLetterCapacityHint > 0 ? _minDeadLetterRetention : Duration.Zero;

    /// <summary>
    /// Specifies how many messages intended for the created listener can be sent by the server to the client at the same time.
    /// </summary>
    /// <remarks>
    /// This is a max potential value. Actual value is dependant on all listeners attached to the queue
    /// and all of its currently pending messages.
    /// </remarks>
    public short PrefetchHint => _prefetchHint ?? DefaultPrefetchHint;

    /// <summary>
    /// Specifies whether or not the client is expected to send ACK or negative ACK to the server in order to confirm message notification.
    /// </summary>
    /// <remarks>
    /// This will always be enabled when <see cref="MaxRetries"/> or <see cref="MaxRedeliveries"/> or <see cref="DeadLetterCapacityHint"/>
    /// is greater than <b>0</b>.
    /// </remarks>
    public bool AreAcksEnabled => MaxRetries > 0 || MaxRedeliveries > 0 || DeadLetterCapacityHint > 0 || ! _acksDisabled;

    /// <summary>
    /// Specifies the delay between the server successfully processing negative ACK sent by the client
    /// and the server sending a message notification retry.
    /// </summary>
    /// <remarks>Equal to <see cref="Duration.Zero"/> when <see cref="MaxRetries"/> is equal to <b>0</b>.</remarks>
    public Duration RetryDelay
    {
        get
        {
            if ( MaxRetries == 0 )
                return Duration.Zero;

            return _retryDelay ?? DefaultRetryDelay;
        }
    }

    /// <summary>
    /// Specifies the minimum amount of time that the server will wait for the client to send either an ACK or a negative ACK
    /// before attempting a message notification redelivery.
    /// Actual ACK timeout may be different due to the state of the queue and other listeners bound to it.
    /// </summary>
    /// <remarks>Equal to <see cref="Duration.Zero"/> when <see cref="AreAcksEnabled"/> is equal to <b>false</b>.</remarks>
    public Duration MinAckTimeout
    {
        get
        {
            if ( ! AreAcksEnabled )
                return Duration.Zero;

            return _minAckTimeout ?? DefaultMinAckTimeout;
        }
    }

    /// <summary>
    /// Allows to change <see cref="PrefetchHint"/>.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="value"/> is not <b>null</b> and is less than <b>1</b>.
    /// </exception>
    [Pure]
    public MessageBrokerListenerOptions SetPrefetchHint(short? value)
    {
        if ( value is not null )
            Ensure.IsGreaterThan( value.Value, 0 );

        return new MessageBrokerListenerOptions(
            value,
            MaxRetries,
            _retryDelay,
            MaxRedeliveries,
            _minAckTimeout,
            DeadLetterCapacityHint,
            _minDeadLetterRetention,
            FilterExpression,
            _acksDisabled );
    }

    /// <summary>
    /// Allows to change <see cref="MaxRetries"/> and <see cref="RetryDelay"/>.
    /// </summary>
    /// <param name="maxRetries">New <see cref="MaxRetries"/> value.</param>
    /// <param name="delay">
    /// Optional new explicit <see cref="RetryDelay"/> value. Sub-millisecond components will be trimmed.
    /// Value will be ignored when <see cref="MaxRetries"/> is equal to <b>0</b>. Equal to <b>null</b> by default.
    /// </param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="maxRetries"/> is less than <b>0</b>
    /// or when <paramref name="delay"/> is not <b>null</b> and is not in [<b>0</b>, <b>2147483647 ms</b>] range.
    /// </exception>
    [Pure]
    public MessageBrokerListenerOptions SetRetryPolicy(int maxRetries, Duration? delay = null)
    {
        Ensure.IsGreaterThanOrEqualTo( maxRetries, 0 );
        if ( delay is not null )
        {
            delay = delay.Value.TrimToMillisecond();
            Ensure.IsInRange( delay.Value, Duration.Zero, Duration.FromMilliseconds( int.MaxValue ) );
        }

        return new MessageBrokerListenerOptions(
            _prefetchHint,
            maxRetries,
            delay,
            MaxRedeliveries,
            _minAckTimeout,
            DeadLetterCapacityHint,
            _minDeadLetterRetention,
            FilterExpression,
            _acksDisabled );
    }

    /// <summary>
    ///  Allows to change <see cref="MaxRedeliveries"/>.
    /// </summary>
    /// <param name="value">New <see cref="MaxRedeliveries"/> value.</param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than <b>0</b>.</exception>
    [Pure]
    public MessageBrokerListenerOptions SetMaxRedeliveries(int value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0 );
        return new MessageBrokerListenerOptions(
            _prefetchHint,
            MaxRetries,
            _retryDelay,
            value,
            _minAckTimeout,
            DeadLetterCapacityHint,
            _minDeadLetterRetention,
            FilterExpression,
            _acksDisabled );
    }

    /// <summary>
    /// Allows to change <see cref="MinAckTimeout"/>.
    /// </summary>
    /// <param name="value">
    /// New explicit <see cref="MinAckTimeout"/> value. Sub-millisecond components will be trimmed.
    /// Value will be ignored when <see cref="AreAcksEnabled"/> is equal to <b>false</b>.
    /// </param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="value"/> is not <b>null</b> and is not in [<b>1 ms</b>, <b>2147483647 ms</b>] range.
    /// </exception>
    [Pure]
    public MessageBrokerListenerOptions SetMinAckTimeout(Duration? value)
    {
        if ( value is not null )
        {
            value = value.Value.TrimToMillisecond();
            Ensure.IsInRange( value.Value, Duration.FromMilliseconds( 1 ), Duration.FromMilliseconds( int.MaxValue ) );
        }

        return new MessageBrokerListenerOptions(
            _prefetchHint,
            MaxRetries,
            _retryDelay,
            MaxRedeliveries,
            value,
            DeadLetterCapacityHint,
            _minDeadLetterRetention,
            FilterExpression,
            _acksDisabled );
    }

    /// <summary>
    /// Allows to change <see cref="DeadLetterCapacityHint"/> and <see cref="MinDeadLetterRetention"/>.
    /// </summary>
    /// <param name="capacityHint">New <see cref="DeadLetterCapacityHint"/> value.</param>
    /// <param name="minRetention">
    /// New <see cref="MinDeadLetterRetention"/> value. Sub-millisecond components will be trimmed.
    /// Value will be ignored when <see cref="DeadLetterCapacityHint"/> is equal to <b>0</b>.
    /// </param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="capacityHint"/> is less than <b>0</b>
    /// or when <paramref name="capacityHint"/> is greater than <b>0</b> and <paramref name="minRetention"/> is less than <b>1 ms</b>
    /// or when <paramref name="capacityHint"/> is equal to <b>0</b> and <paramref name="minRetention"/> is less than <b>0</b>.
    /// </exception>
    [Pure]
    public MessageBrokerListenerOptions SetDeadLetterPolicy(int capacityHint, Duration minRetention)
    {
        Ensure.IsGreaterThanOrEqualTo( capacityHint, 0 );
        minRetention = minRetention.TrimToMillisecond();
        Ensure.IsGreaterThanOrEqualTo( minRetention, capacityHint > 0 ? Duration.FromMilliseconds( 1 ) : Duration.Zero );
        return new MessageBrokerListenerOptions(
            _prefetchHint,
            MaxRetries,
            _retryDelay,
            MaxRedeliveries,
            _minAckTimeout,
            capacityHint,
            minRetention,
            FilterExpression,
            _acksDisabled );
    }

    /// <summary>
    /// Allows to change <see cref="FilterExpression"/>.
    /// </summary>
    /// <param name="value">New <see cref="FilterExpression"/> value.</param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    /// <exception cref="ArgumentException">
    /// When <paramref name="value"/> is not <b>null</b> and consists only of white-space characters.
    /// </exception>
    [Pure]
    public MessageBrokerListenerOptions SetFilterExpression(string? value)
    {
        if ( value is not null )
            Ensure.IsNotNullOrWhiteSpace( value );

        return new MessageBrokerListenerOptions(
            _prefetchHint,
            MaxRetries,
            _retryDelay,
            MaxRedeliveries,
            _minAckTimeout,
            DeadLetterCapacityHint,
            _minDeadLetterRetention,
            value,
            _acksDisabled );
    }

    /// <summary>
    /// Allows to change <see cref="AreAcksEnabled"/>.
    /// </summary>
    /// <param name="enabled">
    /// New <see cref="AreAcksEnabled"/> value.
    /// Value will be ignored when <see cref="MaxRetries"/> or <see cref="MaxRedeliveries"/> or <see cref="DeadLetterCapacityHint"/>
    /// is greater than <b>0</b>.
    /// Equal to <b>true</b> by default.
    /// </param>
    /// <returns>New <see cref="MessageBrokerListenerOptions"/> instance.</returns>
    [Pure]
    public MessageBrokerListenerOptions EnableAcks(bool enabled = true)
    {
        return new MessageBrokerListenerOptions(
            _prefetchHint,
            MaxRetries,
            _retryDelay,
            MaxRedeliveries,
            _minAckTimeout,
            DeadLetterCapacityHint,
            _minDeadLetterRetention,
            FilterExpression,
            ! enabled );
    }
}
