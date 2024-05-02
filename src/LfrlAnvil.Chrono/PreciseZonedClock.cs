using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a precise provider of <see cref="ZonedDateTime"/> instances.
/// </summary>
public sealed class PreciseZonedClock : ZonedClockBase
{
    /// <summary>
    /// <see cref="PreciseZonedClock"/> instance that returns <see cref="ZonedDateTime"/> instances
    /// in the <see cref="TimeZoneInfo.Utc"/> time zone, with <see cref="PrecisionResetTimeout"/> equal to <b>1 minute</b>.
    /// </summary>
    public static readonly PreciseZonedClock Utc = new PreciseZonedClock( TimeZoneInfo.Utc );

    /// <summary>
    /// <see cref="PreciseZonedClock"/> instance that returns <see cref="ZonedDateTime"/> instances
    /// in the <see cref="TimeZoneInfo.Local"/> time zone, with <see cref="PrecisionResetTimeout"/> equal to <b>1 minute</b>.
    /// </summary>
    public static readonly PreciseZonedClock Local = new PreciseZonedClock( TimeZoneInfo.Local );

    private readonly long _maxPreciseMeasurementDuration;
    private Timestamp _timestampStart = new Timestamp( DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks );
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    /// <summary>
    /// Creates a new <see cref="PreciseZonedClock"/> instance with default <see cref="PrecisionResetTimeout"/> equal to <b>1 minute</b>.
    /// </summary>
    /// <param name="timeZone">Time zone of this clock.</param>
    public PreciseZonedClock(TimeZoneInfo timeZone)
        : this( timeZone, Duration.FromMinutes( 1 ) ) { }

    /// <summary>
    /// Creates a new <see cref="PreciseZonedClock"/> instance.
    /// </summary>
    /// <param name="timeZone">Time zone of this clock.</param>
    /// <param name="precisionResetTimeout">Precision reset timeout. See <see cref="PrecisionResetTimeout"/> for more information.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="precisionResetTimeout"/> is less than <b>1 tick</b>.</exception>
    public PreciseZonedClock(TimeZoneInfo timeZone, Duration precisionResetTimeout)
        : base( timeZone )
    {
        _maxPreciseMeasurementDuration = StopwatchTicks.GetStopwatchTicksOrThrow( precisionResetTimeout );
        PrecisionResetTimeout = precisionResetTimeout;
    }

    /// <summary>
    /// Represents a reset timeout for the period of precise <see cref="ZonedDateTime"/> computation.
    /// After this period ends, the next returned <see cref="ZonedDateTime"/> instance will not use the underlying <see cref="Stopwatch"/>
    /// for improved precision and a new timeout will start.
    /// </summary>
    public Duration PrecisionResetTimeout { get; }

    /// <inheritdoc />
    public override ZonedDateTime GetNow()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var preciseMeasurementDuration = currentTimestamp - _preciseMeasurementStart;

        if ( preciseMeasurementDuration > _maxPreciseMeasurementDuration )
        {
            _preciseMeasurementStart = currentTimestamp;
            _timestampStart = new Timestamp( DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks );
            return ZonedDateTime.CreateUtc( _timestampStart ).ToTimeZone( TimeZone );
        }

        var ticksDelta = StopwatchTimestamp.GetTicks( _preciseMeasurementStart, currentTimestamp );
        return ZonedDateTime.CreateUtc( _timestampStart.Add( Duration.FromTicks( ticksDelta ) ) ).ToTimeZone( TimeZone );
    }
}
