using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Internal;

internal static class StopwatchTicks
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static long GetStopwatchTicksOrThrow(Duration duration, string paramName)
    {
        Ensure.IsInExclusiveRange(
            duration,
            Duration.Zero,
            Duration.FromTicks( ChronoConstants.DaysInYear * ChronoConstants.TicksPerStandardDay ),
            paramName );

        var stopwatchTicks = duration.TotalSeconds * Stopwatch.Frequency + 0.5;
        return checked( (long)stopwatchTicks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static long GetDurationTicks(long startStopwatchTicks, long endStopwatchTicks)
    {
        var elapsedSeconds = unchecked( endStopwatchTicks - startStopwatchTicks ) / (double)Stopwatch.Frequency;
        var elapsedTicks = elapsedSeconds * ChronoConstants.TicksPerSecond + 0.5;
        return unchecked( (long)elapsedTicks );
    }
}
