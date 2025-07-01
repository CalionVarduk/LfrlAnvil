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

namespace LfrlAnvil.MessageBroker.Client;

/// <summary>
/// Represents available negative ACK options.
/// </summary>
public readonly struct MessageBrokerNegativeAck
{
    /// <summary>
    /// Represents default negative ACK, with default retry behavior based on <see cref="MessageBrokerListener"/> options.
    /// </summary>
    public static MessageBrokerNegativeAck Default => new MessageBrokerNegativeAck( false, null );

    private MessageBrokerNegativeAck(bool skipRetry, Duration? retryDelay)
    {
        SkipRetry = skipRetry;
        RetryDelay = retryDelay;
    }

    /// <summary>
    /// Specifies whether or not future retries fo the message will be skipped.
    /// </summary>
    public bool SkipRetry { get; }

    /// <summary>
    /// Specifies custom retry delay.
    /// </summary>
    public Duration? RetryDelay { get; }

    /// <summary>
    /// Creates a negative ACK with a custom retry delay.
    /// </summary>
    /// <param name="delay">Custom retry delay. Sub-millisecond components will be trimmed.</param>
    /// <returns>New <see cref="MessageBrokerNegativeAck"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is not in [<b>0</b>, <b>2147483647 ms</b>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerNegativeAck Retry(Duration delay)
    {
        delay = delay.TrimToMillisecond();
        Ensure.IsInRange( delay, Duration.Zero, Duration.FromMilliseconds( int.MaxValue ) );
        return new MessageBrokerNegativeAck( false, delay );
    }

    /// <summary>
    /// Creates a negative ACK that skips any future retries for the message.
    /// </summary>
    /// <returns>New <see cref="MessageBrokerNegativeAck"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerNegativeAck NoRetry()
    {
        return new MessageBrokerNegativeAck( true, null );
    }
}
