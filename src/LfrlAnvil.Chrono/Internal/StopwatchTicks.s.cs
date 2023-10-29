using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono.Internal;

internal static class StopwatchTicks
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static long GetStopwatchTicksOrThrow(Duration duration, [CallerArgumentExpression( "duration" )] string paramName = "")
    {
        Ensure.IsInExclusiveRange(
            duration,
            Duration.Zero,
            Duration.FromTicks( ChronoConstants.DaysInYear * ChronoConstants.TicksPerStandardDay ),
            paramName );

        return StopwatchTimestamp.GetStopwatchTicks( duration.Ticks );
    }
}
