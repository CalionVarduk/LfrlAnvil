using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a precise provider of <see cref="Timestamp"/> instances.
/// </summary>
public sealed class PreciseTimestampProvider : TimestampProviderBase
{
    private readonly long _maxPreciseMeasurementDuration;
    private long _utcStartTicks = DateTime.UtcNow.Ticks - DateTime.UnixEpoch.Ticks;
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    /// <summary>
    /// Creates a new <see cref="PreciseTimestampProvider"/> instance with default <see cref="PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public PreciseTimestampProvider()
        : this( Duration.FromMinutes( 1 ) ) { }

    /// <summary>
    /// Creates a new <see cref="PreciseTimestampProvider"/> instance.
    /// </summary>
    /// <param name="precisionResetTimeout">Precision reset timeout. See <see cref="PrecisionResetTimeout"/> for more information.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="precisionResetTimeout"/> is less than <b>1 tick</b>.</exception>
    public PreciseTimestampProvider(Duration precisionResetTimeout)
    {
        _maxPreciseMeasurementDuration = StopwatchTicks.GetStopwatchTicksOrThrow( precisionResetTimeout );
        PrecisionResetTimeout = precisionResetTimeout;
    }

    /// <summary>
    /// Represents a reset timeout for the period of precise <see cref="Timestamp"/> computation.
    /// After this period ends, the next returned <see cref="Timestamp"/> instance will not use the underlying <see cref="Stopwatch"/>
    /// for improved precision and a new timeout will start.
    /// </summary>
    public Duration PrecisionResetTimeout { get; }

    /// <inheritdoc />
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
