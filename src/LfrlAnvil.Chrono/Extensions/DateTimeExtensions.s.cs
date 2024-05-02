using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="DateTime"/> extension methods.
/// </summary>
public static class DateTimeExtensions
{
    /// <summary>
    /// Gets <see cref="IsoMonthOfYear"/> from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="IsoMonthOfYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IsoMonthOfYear GetMonthOfYear(this DateTime dt)
    {
        return ( IsoMonthOfYear )dt.Month;
    }

    /// <summary>
    /// Gets <see cref="IsoDayOfWeek"/> from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="IsoDayOfWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IsoDayOfWeek GetDayOfWeek(this DateTime dt)
    {
        return dt.DayOfWeek.ToIso();
    }

    /// <summary>
    /// Gets the start of the day from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetStartOfDay(this DateTime dt)
    {
        return dt.Date;
    }

    /// <summary>
    /// Gets the end of the day from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetEndOfDay(this DateTime dt)
    {
        return dt.Date.AddDays( 1 ).AddTicks( -1 );
    }

    /// <summary>
    /// Gets the start of the week from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="weekStart">First day of the week.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetStartOfWeek(this DateTime dt, DayOfWeek weekStart)
    {
        var dayDelta = ( int )dt.DayOfWeek - ( int )weekStart;
        if ( dayDelta < 0 )
            dayDelta += ChronoConstants.DaysPerWeek;

        return (dayDelta > 0 ? dt.AddDays( -dayDelta ) : dt).GetStartOfDay();
    }

    /// <summary>
    /// Gets the end of the week from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="weekStart">First day of the week.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetEndOfWeek(this DateTime dt, DayOfWeek weekStart)
    {
        var dayDelta = ( int )dt.DayOfWeek - ( int )weekStart;
        if ( dayDelta < 0 )
            dayDelta += ChronoConstants.DaysPerWeek;

        dayDelta = -dayDelta + ChronoConstants.DaysPerWeek - 1;

        return (dayDelta > 0 ? dt.AddDays( dayDelta ) : dt).GetEndOfDay();
    }

    /// <summary>
    /// Gets the start of the month from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetStartOfMonth(this DateTime dt)
    {
        return new DateTime( dt.Year, dt.Month, 1 );
    }

    /// <summary>
    /// Gets the end of the month from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetEndOfMonth(this DateTime dt)
    {
        var daysInMonth = DateTime.DaysInMonth( dt.Year, dt.Month );
        return new DateTime( dt.Year, dt.Month, daysInMonth ).GetEndOfDay();
    }

    /// <summary>
    /// Gets the start of the year from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetStartOfYear(this DateTime dt)
    {
        return new DateTime( dt.Year, ( int )IsoMonthOfYear.January, 1 );
    }

    /// <summary>
    /// Gets the end of the year from the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime GetEndOfYear(this DateTime dt)
    {
        return new DateTime( dt.Year, ( int )IsoMonthOfYear.December, ChronoConstants.DaysInDecember ).GetEndOfDay();
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by adding the provided <paramref name="period"/> to the source <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="period"><see cref="Period"/> to add.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    public static DateTime Add(this DateTime dt, Period period)
    {
        var normalizedMonths = period.Years * ChronoConstants.MonthsPerYear + period.Months;

        var normalizedTicks =
            period.Weeks * ChronoConstants.TicksPerStandardWeek
            + period.Days * ChronoConstants.TicksPerStandardDay
            + period.Hours * ChronoConstants.TicksPerHour
            + period.Minutes * ChronoConstants.TicksPerMinute
            + period.Seconds * ChronoConstants.TicksPerSecond
            + period.Milliseconds * ChronoConstants.TicksPerMillisecond
            + period.Microseconds * ChronoConstants.TicksPerMicrosecond
            + period.Ticks;

        var result = dt
            .AddMonths( normalizedMonths )
            .AddTicks( normalizedTicks );

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by subtracting the provided <paramref name="period"/>
    /// from the source <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="period"><see cref="Period"/> to subtract.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime Subtract(this DateTime dt, Period period)
    {
        return dt.Add( -period );
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by setting the <see cref="DateTime.Year"/>
    /// component in the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="year">Year to set.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    public static DateTime SetYear(this DateTime dt, int year)
    {
        var daysInMonth = DateTime.DaysInMonth( year, dt.Month );

        return DateTime.SpecifyKind(
            new DateTime( year, dt.Month, Math.Min( dt.Day, daysInMonth ) ).Add( dt.TimeOfDay ),
            dt.Kind );
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by setting the <see cref="DateTime.Month"/>
    /// component in the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="month">Month to set.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    public static DateTime SetMonth(this DateTime dt, IsoMonthOfYear month)
    {
        var daysInMonth = DateTime.DaysInMonth( dt.Year, ( int )month );

        return DateTime.SpecifyKind(
            new DateTime( dt.Year, ( int )month, Math.Min( dt.Day, daysInMonth ) ).Add( dt.TimeOfDay ),
            dt.Kind );
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by setting the <see cref="DateTime.Day"/> of month
    /// component in the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="day">Day of month to set.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="day"/> is not valid for the current month.</exception>
    [Pure]
    public static DateTime SetDayOfMonth(this DateTime dt, int day)
    {
        return DateTime.SpecifyKind(
            new DateTime( dt.Year, dt.Month, day ).Add( dt.TimeOfDay ),
            dt.Kind );
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by setting the <see cref="DateTime.DayOfYear"/>
    /// component in the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="day">Day of year to set.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="day"/> is not valid for the current year.</exception>
    [Pure]
    public static DateTime SetDayOfYear(this DateTime dt, int day)
    {
        var maxDay = DateTime.IsLeapYear( dt.Year ) ? ChronoConstants.DaysInLeapYear : ChronoConstants.DaysInYear;

        return DateTime.SpecifyKind(
            (day < 1
                ? new DateTime( dt.Year, ( int )IsoMonthOfYear.January, day )
                : day > maxDay
                    ? new DateTime( dt.Year, ( int )IsoMonthOfYear.December, day - maxDay + ChronoConstants.DaysInDecember )
                    : dt.GetStartOfYear().AddDays( day - 1 ))
            .Add( dt.TimeOfDay ),
            dt.Kind );
    }

    /// <summary>
    /// Creates a new <see cref="DateTime"/> instance by setting the <see cref="DateTime.TimeOfDay"/>
    /// component in the provided <paramref name="dt"/>.
    /// </summary>
    /// <param name="dt">Source date time.</param>
    /// <param name="timeOfDay">Time of day to set.</param>
    /// <returns>New <see cref="DateTime"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static DateTime SetTimeOfDay(this DateTime dt, TimeOfDay timeOfDay)
    {
        return dt.GetStartOfDay().Add( ( TimeSpan )timeOfDay );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between the provided <paramref name="start"/>
    /// and <paramref name="end"/> <see cref="DateTime"/> instances, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="end">End date time.</param>
    /// <param name="start">Start date time.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetPeriodOffset(this DateTime end, DateTime start, PeriodUnits units)
    {
        return end < start
            ? PeriodOffsetCalculator.GetPeriodOffset( end, start, units ).Negate()
            : PeriodOffsetCalculator.GetPeriodOffset( start, end, units );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between the provided <paramref name="start"/>
    /// and <paramref name="end"/> <see cref="DateTime"/> instances, using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="end">End date time.</param>
    /// <param name="start">Start date time.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period GetGreedyPeriodOffset(this DateTime end, DateTime start, PeriodUnits units)
    {
        return end < start
            ? PeriodOffsetCalculator.GetGreedyPeriodOffset( end, start, units ).Negate()
            : PeriodOffsetCalculator.GetGreedyPeriodOffset( start, end, units );
    }

    [Pure]
    internal static (ZonedDateTime DateTime, Duration DurationOffset) CreateIntervalStart(
        this DateTime minStartValue,
        TimeZoneInfo timeZone)
    {
        var invalidity = timeZone.GetContainingInvalidityRange( minStartValue );
        if ( invalidity is not null )
        {
            minStartValue = DateTime.SpecifyKind( invalidity.Value.Max.AddTicks( 1 ), minStartValue.Kind );
            return (ZonedDateTime.CreateUnsafe( minStartValue, timeZone ), Duration.Zero);
        }

        var result = ZonedDateTime.CreateUnsafe( minStartValue, timeZone );
        var ambiguousResult = result.GetOppositeAmbiguousDateTime();
        if ( ambiguousResult is null )
            return (result, Duration.Zero);

        var activeRule = timeZone.GetActiveAdjustmentRule( minStartValue );
        Assume.IsNotNull( activeRule );
        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

        var durationOffset = new Duration( transitionTime.TimeOfDay.TimeOfDay )
            .SubtractTicks( activeRule.DaylightDelta.Abs().Ticks );

        return (result.Timestamp < ambiguousResult.Value.Timestamp ? result : ambiguousResult.Value, durationOffset);
    }

    [Pure]
    internal static (ZonedDateTime DateTime, Duration DurationOffset) CreateIntervalEnd(
        this DateTime maxEndValue,
        TimeZoneInfo timeZone)
    {
        var invalidity = timeZone.GetContainingInvalidityRange( maxEndValue );
        if ( invalidity is not null )
        {
            maxEndValue = DateTime.SpecifyKind( invalidity.Value.Min.AddTicks( -1 ), maxEndValue.Kind );
            return (ZonedDateTime.CreateUnsafe( maxEndValue, timeZone ), Duration.Zero);
        }

        var result = ZonedDateTime.CreateUnsafe( maxEndValue, timeZone );
        var ambiguousResult = result.GetOppositeAmbiguousDateTime();
        if ( ambiguousResult is null )
            return (result, Duration.Zero);

        var activeRule = timeZone.GetActiveAdjustmentRule( maxEndValue );
        Assume.IsNotNull( activeRule );
        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();

        var durationOffset = new Duration( -transitionTime.TimeOfDay.TimeOfDay );

        return (result.Timestamp > ambiguousResult.Value.Timestamp ? result : ambiguousResult.Value, durationOffset);
    }
}
