using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a week with time zone.
/// </summary>
public readonly struct ZonedWeek : IEquatable<ZonedWeek>, IComparable<ZonedWeek>, IComparable
{
    private readonly ZonedDateTime? _end;
    private readonly Duration? _duration;

    private ZonedWeek(ZonedDateTime start, ZonedDateTime end, Duration duration)
    {
        Start = start;
        _end = end;
        _duration = duration;
    }

    /// <summary>
    /// Start of this week.
    /// </summary>
    public ZonedDateTime Start { get; }

    /// <summary>
    /// End of this week.
    /// </summary>
    public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.AddTicks( ChronoConstants.TicksPerStandardWeek - 1 ) );

    /// <summary>
    /// Year component.
    /// </summary>
    public int Year => WeekCalculator.GetYearInWeekFormat( Start.Value );

    /// <summary>
    /// Week of year component.
    /// </summary>
    public int WeekOfYear => WeekCalculator.GetWeekOfYear( Start.Value );

    /// <summary>
    /// Time zone of this week.
    /// </summary>
    public TimeZoneInfo TimeZone => Start.TimeZone;

    /// <summary>
    /// <see cref="Chrono.Duration"/> of this week.
    /// </summary>
    public Duration Duration => _duration ?? Duration.FromTicks( ChronoConstants.TicksPerStandardWeek );

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is UTC.
    /// </summary>
    public bool IsUtc => Start.IsUtc;

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is local.
    /// </summary>
    public bool IsLocal => Start.IsLocal;

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    public static ZonedWeek Create(DateTime dateTime, TimeZoneInfo timeZone, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( ( int )weekStart, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );

        var bclWeekStart = weekStart.ToBcl();
        var kind = timeZone.GetDateTimeKind();
        dateTime = DateTime.SpecifyKind( dateTime, kind );

        var (start, startDurationOffset) = dateTime.GetStartOfWeek( bclWeekStart ).CreateIntervalStart( timeZone );
        var (end, endDurationOffset) = dateTime.GetEndOfWeek( bclWeekStart ).CreateIntervalEnd( timeZone );
        var duration = end.GetDurationOffset( start ).Add( startDurationOffset ).Add( endDurationOffset ).AddTicks( 1 );

        return new ZonedWeek( start, end, duration );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek Create(ZonedDateTime dateTime, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return Create( dateTime.Value, dateTime.TimeZone, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance.
    /// </summary>
    /// <param name="day">Day contained by the result.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek Create(ZonedDay day, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return Create( day.Start.Value, day.TimeZone, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance.
    /// </summary>
    /// <param name="year">Year component of the week.</param>
    /// <param name="weekOfYear">Number of the week in year.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="weekOfYear"/> is less than <b>1</b>
    /// or greater than the maximum week number in the specified <paramref name="year"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek Create(int year, int weekOfYear, TimeZoneInfo timeZone, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( ( int )weekStart, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );

        var bclWeekStart = weekStart.ToBcl();
        var maxWeekOfYear = WeekCalculator.GetWeekCountInYear( year, bclWeekStart );
        Ensure.IsInRange( weekOfYear, 1, maxWeekOfYear );

        return CreateUnsafe( year, weekOfYear, timeZone, weekStart );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance.
    /// </summary>
    /// <param name="year">Year component of the week.</param>
    /// <param name="weekOfYear">Number of the week in year.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>
    /// New <see cref="ZonedWeek"/> instance or null when <paramref name="weekOfYear"/> is less than <b>1</b>
    /// or greater than the maximum week number in the specified <paramref name="year"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek? TryCreate(int year, int weekOfYear, TimeZoneInfo timeZone, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( ( int )weekStart, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );

        var bclWeekStart = weekStart.ToBcl();
        var maxWeekOfYear = WeekCalculator.GetWeekCountInYear( year, bclWeekStart );
        if ( weekOfYear <= 0 || weekOfYear > maxWeekOfYear )
            return null;

        return CreateUnsafe( year, weekOfYear, timeZone, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="timestamp">Timestamp contained by the result.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek CreateUtc(Timestamp timestamp, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return CreateUtc( timestamp.UtcValue, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="utcDateTime">Date time contained by the result.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek CreateUtc(DateTime utcDateTime, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( ( int )weekStart, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );

        var bclStartDay = weekStart.ToBcl();
        var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfWeek( bclStartDay ) );
        var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfWeek( bclStartDay ) );

        return new ZonedWeek( start, end, Duration.FromTicks( ChronoConstants.TicksPerStandardWeek ) );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="year">Year component of the week.</param>
    /// <param name="weekOfYear">Number of the week in year.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="weekOfYear"/> is less than <b>1</b>
    /// or greater than the maximum week number in the specified <paramref name="year"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek CreateUtc(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return Create( year, weekOfYear, TimeZoneInfo.Utc, weekStart );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="year">Year component of the week.</param>
    /// <param name="weekOfYear">Number of the week in year.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>
    /// New <see cref="ZonedWeek"/> instance or null when <paramref name="weekOfYear"/> is less than <b>1</b>
    /// or greater than the maximum week number in the specified <paramref name="year"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek? TryCreateUtc(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return TryCreate( year, weekOfYear, TimeZoneInfo.Utc, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="localDateTime">Date time contained by the result.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek CreateLocal(DateTime localDateTime, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return Create( localDateTime, TimeZoneInfo.Local, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="year">Year component of the week.</param>
    /// <param name="weekOfYear">Number of the week in year.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="weekOfYear"/> is less than <b>1</b>
    /// or greater than the maximum week number in the specified <paramref name="year"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek CreateLocal(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return Create( year, weekOfYear, TimeZoneInfo.Local, weekStart );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="year">Year component of the week.</param>
    /// <param name="weekOfYear">Number of the week in year.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>
    /// New <see cref="ZonedWeek"/> instance or null when <paramref name="weekOfYear"/> is less than <b>1</b>
    /// or greater than the maximum week number in the specified <paramref name="year"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek? TryCreateLocal(int year, int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return TryCreate( year, weekOfYear, TimeZoneInfo.Local, weekStart );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ZonedWeek"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var start = Start;
        var end = End;
        var startUtcOffset = start.UtcOffset;
        var endUtcOffset = end.UtcOffset;
        var (year, weekOfYear) = WeekCalculator.GetYearAndWeekOfYear( start.Value );

        var dateText = TextFormatting.StringifyYearAndWeek( year, weekOfYear );
        var startEndDayText = TextFormatting.StringifyWeekStartAndEndDay( start.DayOfWeek, end.DayOfWeek );
        var utcOffsetText = TextFormatting.StringifyOffset( start.UtcOffset );

        if ( startUtcOffset == endUtcOffset )
            return $"{dateText} ({startEndDayText}) {utcOffsetText} ({TimeZone.Id})";

        var endUtcOffsetText = TextFormatting.StringifyOffset( endUtcOffset );
        return $"{dateText} ({startEndDayText}) {utcOffsetText} {endUtcOffsetText} ({TimeZone.Id})";
    }

    /// <inheritdoc />
    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Start.Timestamp ).Add( End.Timestamp ).Add( TimeZone.Id ).Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ZonedWeek d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(ZonedWeek other)
    {
        return Start.Equals( other.Start );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedWeek d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(ZonedWeek other)
    {
        return Start.CompareTo( other.Start );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> in the <paramref name="targetTimeZone"/> from this instance.
    /// </summary>
    /// <param name="targetTimeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        return Create( Start.Value, targetTimeZone, Start.DayOfWeek );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> in <see cref="TimeZoneInfo.Utc"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek ToUtcTimeZone()
    {
        return CreateUtc( Start.Value, Start.DayOfWeek );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> in <see cref="TimeZoneInfo.Local"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> belongs to this week.
    /// </summary>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="dateTime"/> belongs to this week, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(ZonedDateTime dateTime)
    {
        var start = Start;
        var startDate = start.Value.Date;
        var endDate = End.Value.Date;
        var convertedDate = dateTime.ToTimeZone( start.TimeZone ).Value.Date;
        return startDate <= convertedDate && endDate >= convertedDate;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="day"/> belongs to this week.
    /// </summary>
    /// <param name="day">Day to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="day"/> belongs to this week, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDay day)
    {
        return Contains( day.Start ) && (ReferenceEquals( TimeZone, day.TimeZone ) || Contains( day.End ));
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by calculating the next week.
    /// </summary>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetNext()
    {
        return AddWeeks( 1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by calculating the previous week.
    /// </summary>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetPrevious()
    {
        return AddWeeks( -1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by adding provided number of weeks to this instance.
    /// </summary>
    /// <param name="weeks">Number of weeks to add.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek AddWeeks(int weeks)
    {
        var start = Start;
        var value = start.Value.AddTicks( weeks * ChronoConstants.TicksPerStandardWeek );
        return Create( value, start.TimeZone, start.DayOfWeek );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to add.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek Add(Period value)
    {
        var start = Start;
        var dateTime = start.Value.Add( value );
        return Create( dateTime, start.TimeZone, start.DayOfWeek );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by subtracting provided number of weeks from this instance.
    /// </summary>
    /// <param name="weeks">Number of weeks to subtract.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek SubtractWeeks(int weeks)
    {
        return AddWeeks( -weeks );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to subtract.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek Subtract(Period value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start week.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedWeek start, PeriodUnits units)
    {
        return Start.GetPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start week.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedWeek start, PeriodUnits units)
    {
        return Start.GetGreedyPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by setting the <see cref="ZonedWeek.Year"/> component in this instance.
    /// </summary>
    /// <param name="year">Year to set.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    /// <remarks>
    /// Result may end up with modified components other than the year,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek SetYear(int year)
    {
        var start = Start;
        var weekStart = start.DayOfWeek;
        var weekCount = WeekCalculator.GetWeekCountInYear( year, weekStart.ToBcl() );
        return Create( year, Math.Min( WeekOfYear, weekCount ), start.TimeZone, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by setting the <see cref="ZonedWeek.WeekOfYear"/> component in this instance.
    /// </summary>
    /// <param name="week">Week of year to set.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="week"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek SetWeekOfYear(int week)
    {
        var start = Start;
        return Create( Year, week, start.TimeZone, start.DayOfWeek );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance by setting the <see cref="ZonedWeek.WeekOfYear"/>
    /// component in this instance.
    /// </summary>
    /// <param name="week">Week of year to set.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance or null when <paramref name="week"/> is not valid.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek? TrySetWeekOfYear(int week)
    {
        var start = Start;
        return TryCreate( Year, week, start.TimeZone, start.DayOfWeek );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by setting new first day of the week in this instance.
    /// </summary>
    /// <param name="weekStart">First day of the week to set.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="weekStart"/> results in the creation of an invalid week.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek SetWeekStart(IsoDayOfWeek weekStart)
    {
        var start = Start;
        var (year, weekOfYear) = WeekCalculator.GetYearAndWeekOfYear( start.Value );
        return Create( year, weekOfYear, start.TimeZone, weekStart );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance by setting new first day of the week in this instance.
    /// </summary>
    /// <param name="weekStart">First day of the week to set.</param>
    /// <returns>
    /// New <see cref="ZonedWeek"/> instance or null when <paramref name="weekStart"/> results in the creation of an invalid week.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek? TrySetWeekStart(IsoDayOfWeek weekStart)
    {
        var start = Start;
        var (year, weekOfYear) = WeekCalculator.GetYearAndWeekOfYear( start.Value );
        return TryCreate( year, weekOfYear, start.TimeZone, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents the specified <paramref name="day"/> of this week.
    /// </summary>
    /// <param name="day">Day of this week to get.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDayOfWeek(IsoDayOfWeek day)
    {
        Ensure.IsInRange( ( int )day, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );

        var start = Start;

        var offsetInDays = ( int )day - ( int )start.DayOfWeek;
        if ( offsetInDays < 0 )
            offsetInDays += ChronoConstants.DaysPerWeek;

        var dayValue = start.Value.AddTicks( ChronoConstants.TicksPerStandardDay * offsetInDays );
        return ZonedDay.Create( dayValue, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Monday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetMonday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Monday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Tuesday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetTuesday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Tuesday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Wednesday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetWednesday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Wednesday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Thursday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetThursday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Thursday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Friday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetFriday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Friday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Saturday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetSaturday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Saturday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents <see cref="IsoDayOfWeek.Sunday"/> of this week.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetSunday()
    {
        return GetDayOfWeek( IsoDayOfWeek.Sunday );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance that contains this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetYear()
    {
        return ZonedYear.Create( Year, TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all days of this week in order.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<ZonedDay> GetAllDays()
    {
        var start = Start;
        var timeZone = start.TimeZone;

        for ( var dayOffset = 0; dayOffset < ChronoConstants.DaysPerWeek; ++dayOffset )
            yield return ZonedDay.Create( start.Value.AddTicks( ChronoConstants.TicksPerStandardDay * dayOffset ), timeZone );
    }

    /// <summary>
    /// Creates a new <see cref="Bounds{T}"/> instance from this instance's <see cref="Start"/> and <see cref="End"/>.
    /// </summary>
    /// <returns>New <see cref="Bounds{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bounds<ZonedDateTime> ToBounds()
    {
        return Bounds.Create( Start, End );
    }

    /// <summary>
    /// Creates a new <see cref="BoundsRange{T}"/> instance from this instance's <see cref="Start"/> and <see cref="End"/>
    /// including any overlapping ambiguity.
    /// </summary>
    /// <returns>New <see cref="BoundsRange{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<ZonedDateTime> ToCheckedBounds()
    {
        return ZonedDateTimeBounds.CreateChecked( Start, End );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek operator +(ZonedWeek a, Period b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedWeek operator -(ZonedWeek a, Period b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(ZonedWeek a, ZonedWeek b)
    {
        return a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is not equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are not equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(ZonedWeek a, ZonedWeek b)
    {
        return ! a.Equals( b );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(ZonedWeek a, ZonedWeek b)
    {
        return a.CompareTo( b ) > 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(ZonedWeek a, ZonedWeek b)
    {
        return a.CompareTo( b ) <= 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is less than <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is less than <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(ZonedWeek a, ZonedWeek b)
    {
        return a.CompareTo( b ) < 0;
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is greater than or equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when <paramref name="a"/> is greater than or equal to <paramref name="b"/>, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(ZonedWeek a, ZonedWeek b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    private static ZonedWeek CreateUnsafe(int year, int weekOfYear, TimeZoneInfo timeZone, IsoDayOfWeek weekStart)
    {
        var bclWeekStart = weekStart.ToBcl();
        var dayInFirstWeekOfYear = WeekCalculator.GetDayInFirstWeekOfYear( year, bclWeekStart );
        var startOfFirstWeekOfYear = dayInFirstWeekOfYear.GetStartOfWeek( bclWeekStart );
        var startOfTargetWeek = startOfFirstWeekOfYear.AddTicks( ChronoConstants.TicksPerStandardWeek * (weekOfYear - 1) );
        return Create( startOfTargetWeek, timeZone, weekStart );
    }
}
