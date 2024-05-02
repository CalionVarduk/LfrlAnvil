using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="IsoDayOfWeek"/> extension methods.
/// </summary>
public static class IsoDayOfWeekExtensions
{
    /// <summary>
    /// Converts the provided <paramref name="dayOfWeek"/> to <see cref="DayOfWeek"/> type.
    /// </summary>
    /// <param name="dayOfWeek">Value to convert.</param>
    /// <returns>New <see cref="DayOfWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DayOfWeek ToBcl(this IsoDayOfWeek dayOfWeek)
    {
        return ( DayOfWeek )(( int )dayOfWeek % 7);
    }
}
