using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a precise provider of <see cref="DateTime"/> instances with <see cref="DateTimeKind.Utc"/> kind.
/// </summary>
public sealed class PreciseUtcDateTimeProvider : DateTimeProviderBase
{
    private readonly long _maxPreciseMeasurementDuration;
    private DateTime _utcStart = DateTime.UtcNow;
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    /// <summary>
    /// Creates a new <see cref="PreciseUtcDateTimeProvider"/> instance with default <see cref="PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public PreciseUtcDateTimeProvider()
        : this( Duration.FromMinutes( 1 ) ) { }

    /// <summary>
    /// Creates a new <see cref="PreciseUtcDateTimeProvider"/> instance.
    /// </summary>
    /// <param name="precisionResetTimeout">Precision reset timeout. See <see cref="PrecisionResetTimeout"/> for more information.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="precisionResetTimeout"/> is less than <b>1 tick</b>.</exception>
    public PreciseUtcDateTimeProvider(Duration precisionResetTimeout)
        : base( DateTimeKind.Utc )
    {
        _maxPreciseMeasurementDuration = StopwatchTicks.GetStopwatchTicksOrThrow( precisionResetTimeout );
        PrecisionResetTimeout = precisionResetTimeout;
    }

    /// <summary>
    /// Represents a reset timeout for the period of precise <see cref="DateTime"/> computation.
    /// After this period ends, the next returned <see cref="DateTime"/> instance will not use the underlying <see cref="Stopwatch"/>
    /// for improved precision and a new timeout will start.
    /// </summary>
    public Duration PrecisionResetTimeout { get; }

    /// <inheritdoc />
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

        var ticksDelta = StopwatchTimestamp.GetTicks( _preciseMeasurementStart, currentTimestamp );
        return _utcStart.AddTicks( ticksDelta );
    }
}
