using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
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
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval)
    {
        return Interval( timestampProvider, interval, ReactiveTimer.DefaultSpinWaitDurationHint, count: long.MaxValue );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="count">Number of events underlying timers will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval, long count)
    {
        return Interval( timestampProvider, interval, ReactiveTimer.DefaultSpinWaitDurationHint, count );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval, Duration spinWaitDurationHint)
    {
        return Interval( timestampProvider, interval, spinWaitDurationHint, count: long.MaxValue );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <param name="count">Number of events underlying timers will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        ITimestampProvider timestampProvider,
        Duration interval,
        Duration spinWaitDurationHint,
        long count)
    {
        return new IntervalEventSource( timestampProvider, interval, scheduler: null, spinWaitDurationHint, count );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval, TaskScheduler scheduler)
    {
        return Interval( timestampProvider, interval, scheduler, ReactiveTimer.DefaultSpinWaitDurationHint, count: long.MaxValue );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <param name="count">Number of events underlying timers will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        ITimestampProvider timestampProvider,
        Duration interval,
        TaskScheduler scheduler,
        long count)
    {
        return Interval( timestampProvider, interval, scheduler, ReactiveTimer.DefaultSpinWaitDurationHint, count );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        ITimestampProvider timestampProvider,
        Duration interval,
        TaskScheduler scheduler,
        Duration spinWaitDurationHint)
    {
        return Interval( timestampProvider, interval, scheduler, spinWaitDurationHint, count: long.MaxValue );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="interval">Interval between subsequent timer events.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <param name="count">Number of events underlying timers will emit in total. Equal to <see cref="Int64.MaxValue"/> by default.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="count"/> is less than <b>1</b>
    /// or when <paramref name="interval"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(
        ITimestampProvider timestampProvider,
        Duration interval,
        TaskScheduler scheduler,
        Duration spinWaitDurationHint,
        long count)
    {
        return new IntervalEventSource( timestampProvider, interval, scheduler, spinWaitDurationHint, count );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance with a single emitted event.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="timeout">Delay before timer event is emitted.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="timeout"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(ITimestampProvider timestampProvider, Duration timeout)
    {
        return Timeout( timestampProvider, timeout, ReactiveTimer.DefaultSpinWaitDurationHint );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance with a single emitted event.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="timeout">Delay before timer event is emitted.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="timeout"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(ITimestampProvider timestampProvider, Duration timeout, Duration spinWaitDurationHint)
    {
        return Interval( timestampProvider, timeout, spinWaitDurationHint, count: 1 );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance with a single emitted event.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="timeout">Delay before timer event is emitted.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="timeout"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(ITimestampProvider timestampProvider, Duration timeout, TaskScheduler scheduler)
    {
        return Timeout( timestampProvider, timeout, scheduler, ReactiveTimer.DefaultSpinWaitDurationHint );
    }

    /// <summary>
    /// Creates a new <see cref="IntervalEventSource"/> instance with a single emitted event.
    /// </summary>
    /// <param name="timestampProvider">Timestamp provider used for time tracking.</param>
    /// <param name="timeout">Delay before timer event is emitted.</param>
    /// <param name="scheduler">Task scheduler.</param>
    /// <param name="spinWaitDurationHint"><see cref="SpinWait"/> duration hint for underlying timers.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="timeout"/> is less than <b>1 tick</b> or greater than <see cref="Int32.MaxValue"/> milliseconds
    /// or when <paramref name="spinWaitDurationHint"/> is less than <b>0</b>.
    /// </exception>
    /// <returns>New <see cref="IntervalEventSource"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(
        ITimestampProvider timestampProvider,
        Duration timeout,
        TaskScheduler scheduler,
        Duration spinWaitDurationHint)
    {
        return Interval( timestampProvider, timeout, scheduler, spinWaitDurationHint, count: 1 );
    }
}
