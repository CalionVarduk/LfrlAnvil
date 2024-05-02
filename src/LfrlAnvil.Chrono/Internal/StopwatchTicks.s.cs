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
        Ensure.IsGreaterThan( duration, Duration.Zero, paramName );
        return StopwatchTimestamp.GetStopwatchTicks( duration.Ticks );
    }
}
