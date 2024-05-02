using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="TimeSpan"/> extension methods.
/// </summary>
public static class TimeSpanExtensions
{
    /// <summary>
    /// Creates a new <see cref="TimeSpan"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <param name="ts">Source timespan.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan Abs(this TimeSpan ts)
    {
        return TimeSpan.FromTicks( Math.Abs( ts.Ticks ) );
    }
}
