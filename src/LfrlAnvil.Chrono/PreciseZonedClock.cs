using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

public sealed class PreciseZonedClock : ZonedClockBase
{
    public static readonly PreciseZonedClock Utc = new PreciseZonedClock( TimeZoneInfo.Utc );
    public static readonly PreciseZonedClock Local = new PreciseZonedClock( TimeZoneInfo.Local );

    private readonly long _maxPreciseMeasurementDuration;
    private Timestamp _timestampStart = new Timestamp( DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks );
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    public PreciseZonedClock(TimeZoneInfo timeZone)
        : this( timeZone, Duration.FromMinutes( 1 ) ) { }

    public PreciseZonedClock(TimeZoneInfo timeZone, Duration precisionResetTimeout)
        : base( timeZone )
    {
        _maxPreciseMeasurementDuration = StopwatchTicks.GetStopwatchTicksOrThrow( precisionResetTimeout );
        PrecisionResetTimeout = precisionResetTimeout;
    }

    public Duration PrecisionResetTimeout { get; }

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
