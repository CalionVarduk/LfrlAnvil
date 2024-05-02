using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="DayOfWeek"/> extension methods.
/// </summary>
public static class DayOfWeekExtensions
{
    /// <summary>
    /// Converts the provided <paramref name="dayOfWeek"/> to <see cref="IsoDayOfWeek"/> type.
    /// </summary>
    /// <param name="dayOfWeek">Value to convert.</param>
    /// <returns>New <see cref="IsoDayOfWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IsoDayOfWeek ToIso(this DayOfWeek dayOfWeek)
    {
        return dayOfWeek == DayOfWeek.Sunday
            ? IsoDayOfWeek.Sunday
            : ( IsoDayOfWeek )dayOfWeek;
    }
}
