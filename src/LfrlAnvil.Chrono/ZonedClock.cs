using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

/// <inheritdoc cref="ZonedClockBase" />
public sealed class ZonedClock : ZonedClockBase
{
    /// <summary>
    /// <see cref="ZonedClock"/> instance that returns <see cref="ZonedDateTime"/> instances
    /// in the <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    public static readonly ZonedClock Utc = new ZonedClock( TimeZoneInfo.Utc );

    /// <summary>
    /// <see cref="ZonedClock"/> instance that returns <see cref="ZonedDateTime"/> instances
    /// in the <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    public static readonly ZonedClock Local = new ZonedClock( TimeZoneInfo.Local );

    /// <summary>
    /// Creates a new <see cref="ZonedClock"/> instance.
    /// </summary>
    /// <param name="timeZone">Time zone of this clock.</param>
    public ZonedClock(TimeZoneInfo timeZone)
        : base( timeZone ) { }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override ZonedDateTime GetNow()
    {
        return ZonedDateTime.CreateUtc( DateTime.UtcNow ).ToTimeZone( TimeZone );
    }
}
