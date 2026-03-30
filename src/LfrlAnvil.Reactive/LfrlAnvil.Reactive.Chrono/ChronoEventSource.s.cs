// Copyright 2024-2026 Łukasz Furlepa
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
using System.Threading;
using LfrlAnvil.Chrono;
using LfrlAnvil.Chrono.Async;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

/// <summary>
/// Creates instances of <see cref="IntervalEventSource"/> type.
/// </summary>
public static class ChronoEventSource
{
    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        Duration interval,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null)
    {
        return Interval( interval, ReactiveTimer.DefaultSpinWaitDurationHint, count: long.MaxValue, timestampProvider, delaySource );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="count">Number of events underlying timers will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        Duration interval,
        long count,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null)
    {
        return Interval( interval, ReactiveTimer.DefaultSpinWaitDurationHint, count, timestampProvider, delaySource );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        Duration interval,
        Duration spinWaitDurationHint,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null)
    {
        return Interval( interval, spinWaitDurationHint, count: long.MaxValue, timestampProvider, delaySource );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <param name="count">Number of events underlying timers will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        Duration interval,
        Duration spinWaitDurationHint,
        long count,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null)
    {
        return new IntervalEventSource( timestampProvider, interval, spinWaitDurationHint, delaySource, count );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance with a single emitted event.
    /// </summary>
    /// <param name="timeout">Delay before timer event is emitted.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="timeout"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(
        Duration timeout,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null)
    {
        return Timeout( timeout, ReactiveTimer.DefaultSpinWaitDurationHint, timestampProvider, delaySource );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance with a single emitted event.
    /// </summary>
    /// <param name="timeout">Delay before timer event is emitted.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <param name="timestampProvider">Optional timestamp provider used for time tracking.</param>
    /// <param name="delaySource">Optional value task delay source to use for scheduling delays.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="timeout"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(
        Duration timeout,
        Duration spinWaitDurationHint,
        ITimestampProvider? timestampProvider = null,
        ValueTaskDelaySource? delaySource = null)
    {
        return Interval( timeout, spinWaitDurationHint, count: 1, timestampProvider, delaySource );
    }
}
