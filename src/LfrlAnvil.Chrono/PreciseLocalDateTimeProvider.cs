using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

public sealed class PreciseLocalDateTimeProvider : DateTimeProviderBase
{
    private readonly long _maxPreciseMeasurementDuration;
    private DateTime _localStart = DateTime.Now;
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    public PreciseLocalDateTimeProvider()
        : this( Duration.FromMinutes( 1 ) ) { }

    public PreciseLocalDateTimeProvider(Duration precisionResetTimeout)
        : base( DateTimeKind.Local )
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
            _localStart = DateTime.Now;
            return _localStart;
        }

        var ticksDelta = StopwatchTimestamp.GetTicks( _preciseMeasurementStart, currentTimestamp );
        return _localStart.AddTicks( ticksDelta );
    }
}
