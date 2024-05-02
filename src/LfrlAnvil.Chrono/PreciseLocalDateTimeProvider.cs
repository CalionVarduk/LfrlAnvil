using System;
using System.Diagnostics;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Diagnostics;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a precise provider of <see cref="DateTime"/> instances with <see cref="DateTimeKind.Local"/> kind.
/// </summary>
public sealed class PreciseLocalDateTimeProvider : DateTimeProviderBase
{
    private readonly long _maxPreciseMeasurementDuration;
    private DateTime _localStart = DateTime.Now;
    private long _preciseMeasurementStart = Stopwatch.GetTimestamp();

    /// <summary>
    /// Creates a new <see cref="PreciseLocalDateTimeProvider"/> instance with default <see cref="PrecisionResetTimeout"/>
    /// equal to <b>1 minute</b>.
    /// </summary>
    public PreciseLocalDateTimeProvider()
        : this( Duration.FromMinutes( 1 ) ) { }

    /// <summary>
    /// Creates a new <see cref="PreciseLocalDateTimeProvider"/> instance.
    /// </summary>
    /// <param name="precisionResetTimeout">Precision reset timeout. See <see cref="PrecisionResetTimeout"/> for more information.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="precisionResetTimeout"/> is less than <b>1 tick</b>.</exception>
    public PreciseLocalDateTimeProvider(Duration precisionResetTimeout)
        : base( DateTimeKind.Local )
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
            _localStart = DateTime.Now;
            return _localStart;
        }

        var ticksDelta = StopwatchTimestamp.GetTicks( _preciseMeasurementStart, currentTimestamp );
        return _localStart.AddTicks( ticksDelta );
    }
}
