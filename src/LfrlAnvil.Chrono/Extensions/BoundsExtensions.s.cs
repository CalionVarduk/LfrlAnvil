using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="Bounds{T}"/> extension methods.
/// </summary>
public static class BoundsExtensions
{
    /// <summary>
    /// Calculates the <see cref="TimeSpan"/> of the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TimeSpan GetTimeSpan(this Bounds<DateTime> source)
    {
        return source.Max - source.Min + TimeSpan.FromTicks( 1 );
    }

    /// <summary>
    /// Calculates the <see cref="Period"/> of the provided <paramref name="source"/>, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetPeriod(this Bounds<DateTime> source, PeriodUnits units)
    {
        return (source.Max + TimeSpan.FromTicks( 1 )).GetPeriodOffset( source.Min, units );
    }

    /// <summary>
    /// Calculates the <see cref="Period"/> of the provided <paramref name="source"/>, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetGreedyPeriod(this Bounds<DateTime> source, PeriodUnits units)
    {
        return (source.Max + TimeSpan.FromTicks( 1 )).GetGreedyPeriodOffset( source.Min, units );
    }

    /// <summary>
    /// Calculates the <see cref="Duration"/> of the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration GetDuration(this Bounds<ZonedDateTime> source)
    {
        return source.Max.GetDurationOffset( source.Min ).AddTicks( 1 );
    }

    /// <summary>
    /// Calculates the <see cref="Period"/> of the provided <paramref name="source"/>, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetPeriod(this Bounds<ZonedDateTime> source, PeriodUnits units)
    {
        return source.Max.Add( Duration.FromTicks( 1 ) ).GetPeriodOffset( source.Min, units );
    }

    /// <summary>
    /// Calculates the <see cref="Period"/> of the provided <paramref name="source"/>, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetGreedyPeriod(this Bounds<ZonedDateTime> source, PeriodUnits units)
    {
        return source.Max.Add( Duration.FromTicks( 1 ) ).GetGreedyPeriodOffset( source.Min, units );
    }

    /// <summary>
    /// Calculates the <see cref="Period"/> of the provided <paramref name="source"/>, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetPeriod(this Bounds<ZonedDay> source, PeriodUnits units)
    {
        return source.Max.End.Add( Duration.FromTicks( 1 ) ).GetPeriodOffset( source.Min.Start, units );
    }

    /// <summary>
    /// Calculates the <see cref="Period"/> of the provided <paramref name="source"/>, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="source">Source bounds.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetGreedyPeriod(this Bounds<ZonedDay> source, PeriodUnits units)
    {
        return source.Max.End.Add( Duration.FromTicks( 1 ) ).GetGreedyPeriodOffset( source.Min.Start, units );
    }
}
