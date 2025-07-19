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
    public static MessageBrokerNegativeAck Default => new MessageBrokerNegativeAck( false, false, null );

    internal MessageBrokerNegativeAck(bool skipRetry, bool skipDeadLetter, Duration? retryDelay)
    {
        SkipRetry = skipRetry;
        SkipDeadLetter = skipDeadLetter;
        RetryDelay = retryDelay;
    }

    /// <summary>
    /// Specifies whether or not future retries for the message will be skipped.
    /// </summary>
    public bool SkipRetry { get; }

    /// <summary>
    /// Specifies whether or not the last failed message attempt should not add it to dead letter.
    /// </summary>
    public bool SkipDeadLetter { get; }

    /// <summary>
    /// Specifies custom retry delay.
    /// </summary>
    public Duration? RetryDelay { get; }

    /// <summary>
    /// Creates a negative ACK with a custom retry delay.
    /// </summary>
    /// <param name="delay">Custom retry delay. Sub-millisecond components will be trimmed.</param>
    /// <param name="skipDeadLetter">
    /// Specifies whether or not the last failed message attempt should not add it to dead letter. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="MessageBrokerNegativeAck"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="delay"/> is not in [<b>0</b>, <b>2147483647 ms</b>] range.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerNegativeAck Retry(Duration delay, bool skipDeadLetter = false)
    {
        delay = delay.TrimToMillisecond();
        Ensure.IsInRange( delay, Duration.Zero, Duration.FromMilliseconds( int.MaxValue ) );
        return new MessageBrokerNegativeAck( false, skipDeadLetter, delay );
    }

    /// <summary>
    /// Creates a negative ACK that skips any future retries for the message.
    /// </summary>
    /// <param name="skipDeadLetter">
    /// Specifies whether or not the last failed message attempt should not add it to dead letter. Equal to <b>false</b> by default.
    /// </param>
    /// <returns>New <see cref="MessageBrokerNegativeAck"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerNegativeAck NoRetry(bool skipDeadLetter = false)
    {
        return new MessageBrokerNegativeAck( true, skipDeadLetter, null );
    }

    /// <summary>
    /// Creates a negative ACK that skips last failed message attempt to add it to dead letter.
    /// </summary>
    /// <returns>New <see cref="MessageBrokerNegativeAck"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MessageBrokerNegativeAck NoDeadLetter()
    {
        return new MessageBrokerNegativeAck( false, true, null );
    }
}
