using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using LfrlAnvil.Chrono;
using LfrlAnvil.Reactive.Chrono.Internal;

namespace LfrlAnvil.Reactive.Chrono;

public static class ChronoEventSource
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval)
    {
        return Interval( timestampProvider, interval, ReactiveTimer.DefaultSpinWaitDurationHint, count: long.MaxValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval, long count)
    {
        return Interval( timestampProvider, interval, ReactiveTimer.DefaultSpinWaitDurationHint, count );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval, Duration spinWaitDurationHint)
    {
        return Interval( timestampProvider, interval, spinWaitDurationHint, count: long.MaxValue );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Interval(ITimestampProvider timestampProvider, Duration interval, TaskScheduler scheduler)
    {
        return Interval( timestampProvider, interval, scheduler, ReactiveTimer.DefaultSpinWaitDurationHint, count: long.MaxValue );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(ITimestampProvider timestampProvider, Duration timeout)
    {
        return Timeout( timestampProvider, timeout, ReactiveTimer.DefaultSpinWaitDurationHint );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(ITimestampProvider timestampProvider, Duration timeout, Duration spinWaitDurationHint)
    {
        return Interval( timestampProvider, timeout, spinWaitDurationHint, count: 1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IntervalEventSource Timeout(ITimestampProvider timestampProvider, Duration timeout, TaskScheduler scheduler)
    {
        return Timeout( timestampProvider, timeout, scheduler, ReactiveTimer.DefaultSpinWaitDurationHint );
    }

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
