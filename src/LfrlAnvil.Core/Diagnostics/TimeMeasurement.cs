using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Diagnostics;

/// <summary>
/// A lightweight object that contains the result of <see cref="Measurable"/> invocation's time measurement.
/// </summary>
public readonly struct TimeMeasurement
{
    /// <summary>
    /// Empty measurement.
    /// </summary>
    public static readonly TimeMeasurement Zero = new TimeMeasurement();

    /// <summary>
    /// Creates a new <see cref="TimeMeasurement"/> instance.
    /// </summary>
    /// <param name="preparation">Time elapsed in the <see cref="MeasurableState.Preparing"/> stage.</param>
    /// <param name="invocation">Time elapsed in the <see cref="MeasurableState.Running"/> stage.</param>
    /// <param name="teardown">Time elapsed in the <see cref="MeasurableState.TearingDown"/> stage.</param>
    public TimeMeasurement(TimeSpan preparation, TimeSpan invocation, TimeSpan teardown)
    {
        Preparation = preparation;
        Invocation = invocation;
        Teardown = teardown;
    }

    /// <summary>
    /// Time elapsed in the <see cref="MeasurableState.Preparing"/> stage.
    /// </summary>
    public TimeSpan Preparation { get; }

    /// <summary>
    /// Time elapsed in the <see cref="MeasurableState.Running"/> stage.
    /// </summary>
    public TimeSpan Invocation { get; }

    /// <summary>
    /// Time elapsed in the <see cref="MeasurableState.TearingDown"/> stage.
    /// </summary>
    public TimeSpan Teardown { get; }

    /// <summary>
    /// Total time elapsed.
    /// </summary>
    public TimeSpan Total => Preparation + Invocation + Teardown;

    /// <summary>
    /// Returns a string representation of this <see cref="TimeMeasurement"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var preparation = $"{nameof( Preparation )}: {Stringify( Preparation )}";
        var invocation = $"{nameof( Invocation )}: {Stringify( Invocation )}";
        var teardown = $"{nameof( Teardown )}: {Stringify( Teardown )}";
        var total = $"{nameof( Total )}: {Stringify( Total )}";
        return $"{preparation}, {invocation}, {teardown} ({total})";
    }

    /// <summary>
    /// Creates a new <see cref="TimeMeasurement"/> instance with updated <see cref="Preparation"/>.
    /// </summary>
    /// <param name="preparation">Time elapsed in the <see cref="MeasurableState.Preparing"/> stage.</param>
    /// <returns>New <see cref="TimeMeasurement"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeMeasurement SetPreparation(TimeSpan preparation)
    {
        return new TimeMeasurement( preparation, Invocation, Teardown );
    }

    /// <summary>
    /// Creates a new <see cref="TimeMeasurement"/> instance with updated <see cref="Invocation"/>.
    /// </summary>
    /// <param name="invocation">Time elapsed in the <see cref="MeasurableState.Running"/> stage.</param>
    /// <returns>New <see cref="TimeMeasurement"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeMeasurement SetInvocation(TimeSpan invocation)
    {
        return new TimeMeasurement( Preparation, invocation, Teardown );
    }

    /// <summary>
    /// Creates a new <see cref="TimeMeasurement"/> instance with updated <see cref="Teardown"/>.
    /// </summary>
    /// <param name="teardown">Time elapsed in the <see cref="MeasurableState.TearingDown"/> stage.</param>
    /// <returns>New <see cref="TimeMeasurement"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeMeasurement SetTeardown(TimeSpan teardown)
    {
        return new TimeMeasurement( Preparation, Invocation, teardown );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static string Stringify(TimeSpan timeSpan)
    {
        return $"{timeSpan.TotalSeconds.ToString( "N7", CultureInfo.InvariantCulture )}s";
    }
}
