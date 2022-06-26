using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

public static class TimeSpanExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan Abs(this TimeSpan ts)
    {
        return TimeSpan.FromTicks( Math.Abs( ts.Ticks ) );
    }
}