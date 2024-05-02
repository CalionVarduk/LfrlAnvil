using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Exceptions;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="IZonedClock"/> extension methods.
/// </summary>
public static class ZonedClockExtensions
{
    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance from the provided <paramref name="dateTime"/>
    /// and with the time zone of the given <paramref name="clock"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="dateTime">Date time value.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">
    /// When <paramref name="dateTime"/> is not a valid date time in the clock's time zone.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime Create(this IZonedClock clock, DateTime dateTime)
    {
        return ZonedDateTime.Create( dateTime, clock.TimeZone );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance from the provided <paramref name="dateTime"/>
    /// and with the time zone of the given <paramref name="clock"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="dateTime">Date time value.</param>
    /// <returns>
    /// New <see cref="ZonedDateTime"/> instance or null when <paramref name="dateTime"/> is not a valid date time in the clock's time zone.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDateTime? TryCreate(this IZonedClock clock, DateTime dateTime)
    {
        return ZonedDateTime.TryCreate( dateTime, clock.TimeZone );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> is in the past
    /// relative to the given <paramref name="clock"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when <paramref name="dateTime"/> is in the past, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInPast(this IZonedClock clock, ZonedDateTime dateTime)
    {
        return dateTime.Timestamp < clock.GetNow().Timestamp;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> is in the present
    /// relative to the given <paramref name="clock"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when <paramref name="dateTime"/> is in the present, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNow(this IZonedClock clock, ZonedDateTime dateTime)
    {
        return dateTime.Timestamp == clock.GetNow().Timestamp;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> is in the future
    /// relative to the given <paramref name="clock"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when <paramref name="dateTime"/> is in the future, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsInFuture(this IZonedClock clock, ZonedDateTime dateTime)
    {
        return dateTime.Timestamp > clock.GetNow().Timestamp;
    }

    /// <summary>
    /// Checks whether or not the given <paramref name="clock"/> is frozen.
    /// </summary>
    /// <param name="clock">Clock to check.</param>
    /// <returns><b>true</b> when <paramref name="clock"/> is frozen, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsFrozen(this IZonedClock clock)
    {
        return clock is FrozenZonedClock;
    }

    /// <summary>
    /// Calculates an offset between the provided <paramref name="dateTime"/>
    /// and the current <see cref="ZonedDateTime"/> of the given <paramref name="clock"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration GetDurationOffset(this IZonedClock clock, ZonedDateTime dateTime)
    {
        return dateTime.GetDurationOffset( clock.GetNow() );
    }

    /// <summary>
    /// Calculates an offset between current <see cref="ZonedDateTime"/> instances of two clocks.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <param name="other">Other clock to check.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration GetDurationOffset(this IZonedClock clock, IZonedClock other)
    {
        return clock.GetDurationOffset( other.GetNow() );
    }

    /// <summary>
    /// Creates a new frozen <see cref="IZonedClock"/> instance from the given <paramref name="clock"/>,
    /// using its current <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="clock">Source clock.</param>
    /// <returns>New frozen <see cref="IZonedClock"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IZonedClock Freeze(this IZonedClock clock)
    {
        return new FrozenZonedClock( clock.GetNow() );
    }
}
