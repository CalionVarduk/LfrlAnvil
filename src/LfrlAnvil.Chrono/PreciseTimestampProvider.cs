using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

public sealed class PreciseTimestampProvider : TimestampProviderBase
{
    private readonly long _maxPreciseMeasurementDuration;
    private long _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    public PreciseTimestampProvider()
        : this( Duration.FromMinutes( 1 ) ) { }

    public PreciseTimestampProvider(Duration precisionResetTimeout)
    {
        _maxPreciseMeasurementDuration = StopwatchTicks.GetStopwatchTicksOrThrow( precisionResetTimeout, nameof( precisionResetTimeout ) );
        PrecisionResetTimeout = precisionResetTimeout;
    }

    public Duration PrecisionResetTimeout { get; }

    public override Timestamp GetNow()
    {
        var currentTimestamp = Stopwatch.GetTimestamp();
        var preciseMeasurementDuration = currentTimestamp - _preciseMeasurementStart;

        if ( preciseMeasurementDuration > _maxPreciseMeasurementDuration )
        {
            _preciseMeasurementStart = currentTimestamp;
            _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
            return new Timestamp( _utcStartTicks );
        }

        var ticksDelta = StopwatchTimestamp.GetTicks( _preciseMeasurementStart, currentTimestamp );
        return new Timestamp( _utcStartTicks + ticksDelta );
    }
}
