using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

public static class StopwatchTimestamp
{
    public static readonly double TicksPerStopwatchTick = (double)TimeSpan.TicksPerSecond / Stopwatch.Frequency;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetTicks(long start, long end)
    {
        var elapsedTicks = unchecked( end - start ) * TicksPerStopwatchTick;
        return unchecked( (long)elapsedTicks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan GetTimeSpan(long start, long end)
    {
        return TimeSpan.FromTicks( GetTicks( start, end ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetStopwatchTicks(long ticks)
    {
        var stopwatchTicks = ticks / TicksPerStopwatchTick;
        return checked( (long)stopwatchTicks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static long GetStopwatchTicks(TimeSpan timeSpan)
    {
        return GetStopwatchTicks( timeSpan.Ticks );
    }
}
