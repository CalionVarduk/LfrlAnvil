using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public sealed class PreciseUtcDateTimeProvider : DateTimeProviderBase
{
    private readonly long _maxPreciseMeasurementDuration;
    private DateTime _utcStart = DateTime.UtcNow;
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    public PreciseUtcDateTimeProvider()
        : this( Duration.FromMinutes( 1 ) ) { }

    public PreciseUtcDateTimeProvider(Duration precisionResetTimeout)
        : base( DateTimeKind.Utc )
    {
        _maxPreciseMeasurementDuration = StopwatchTicks.GetStopwatchTicksOrThrow( precisionResetTimeout, nameof( precisionResetTimeout ) );
        PrecisionResetTimeout = precisionResetTimeout;
    }

    public Duration PrecisionResetTimeout { get; }

    public override DateTime GetNow()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var preciseMeasurementDuration = currentTimestamp - _preciseMeasurementStart;

        if ( preciseMeasurementDuration > _maxPreciseMeasurementDuration )
        {
            _preciseMeasurementStart = currentTimestamp;
            _utcStart = DateTime.UtcNow;
            return _utcStart;
        }

        var ticksDelta = StopwatchTicks.GetDurationTicks( _preciseMeasurementStart, currentTimestamp );
        return _utcStart.AddTicks( ticksDelta );
    }
}
