using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a year with time zone.
/// </summary>
public readonly struct ZonedYear : IEquatable<ZonedYear>, IComparable<ZonedYear>, IComparable
{
    private readonly ZonedDateTime? _end;
    private readonly Duration? _duration;

    private ZonedYear(ZonedDateTime start, ZonedDateTime end, Duration duration)
    {
        Start = start;
        _end = end;
        _duration = duration;
    }

    /// <summary>
    /// Start of this year.
    /// </summary>
    public ZonedDateTime Start { get; }

    /// <summary>
    /// End of this year.
    /// </summary>
    public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfYear() );

    /// <summary>
    /// Year component.
    /// </summary>
    public int Year => Start.Year;

    /// <summary>
    /// Specifies whether or not this year is a leap year.
    /// </summary>
    public bool IsLeap => DateTime.IsLeapYear( Year );

    /// <summary>
    /// Number of days in this year.
    /// </summary>
    public int DayCount => IsLeap ? ChronoConstants.DaysInLeapYear : ChronoConstants.DaysInYear;

    /// <summary>
    /// Time zone of this year.
    /// </summary>
    public TimeZoneInfo TimeZone => Start.TimeZone;

    /// <summary>
    /// <see cref="Chrono.Duration"/> of this year.
    /// </summary>
    public Duration Duration => _duration ?? Duration.FromHours( ChronoConstants.HoursPerStandardDay * ChronoConstants.DaysInYear );

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is UTC.
    /// </summary>
    public bool IsUtc => Start.IsUtc;

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is local.
    /// </summary>
    public bool IsLocal => Start.IsLocal;

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    public static ZonedYear Create(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var kind = timeZone.GetDateTimeKind();
        dateTime = DateTime.SpecifyKind( dateTime, kind );

        var (start, startDurationOffset) = dateTime.GetStartOfYear().CreateIntervalStart( timeZone );
        var (end, endDurationOffset) = dateTime.GetEndOfYear().CreateIntervalEnd( timeZone );
        var duration = end.GetDurationOffset( start ).Add( startDurationOffset ).Add( endDurationOffset ).AddTicks( 1 );

        return new ZonedYear( start, end, duration );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(ZonedDateTime dateTime)
    {
        return Create( dateTime.Value, dateTime.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance.
    /// </summary>
    /// <param name="day">Day contained by the result.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(ZonedDay day)
    {
        return Create( day.Start.Value, day.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance.
    /// </summary>
    /// <param name="month">Month contained by the result.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(ZonedMonth month)
    {
        return Create( month.Start.Value, month.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance.
    /// </summary>
    /// <param name="year">Year component.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(int year, TimeZoneInfo timeZone)
    {
        return Create( new DateTime( year, ( int )IsoMonthOfYear.January, 1 ), timeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="timestamp">Timestamp contained by the result.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateUtc(Timestamp timestamp)
    {
        return CreateUtc( timestamp.UtcValue );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="utcDateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateUtc(DateTime utcDateTime)
    {
        var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfYear() );
        var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfYear() );
        return new ZonedYear( start, end, end.GetDurationOffset( start ).AddTicks( 1 ) );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="year">Year component.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateUtc(int year)
    {
        return CreateUtc( new DateTime( year, ( int )IsoMonthOfYear.January, 1 ) );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="localDateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateLocal(DateTime localDateTime)
    {
        return Create( localDateTime, TimeZoneInfo.Local );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="year">Year component.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateLocal(int year)
    {
        return Create( new DateTime( year, ( int )IsoMonthOfYear.January, 1 ), TimeZoneInfo.Local );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ZonedYear"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var start = Start;
        var timeZone = start.TimeZone;

        var yearText = TextFormatting.StringifyYear( start.Value.Year );
        var utcOffsetText = TextFormatting.StringifyOffset( new Duration( timeZone.BaseUtcOffset ) );

        return $"{yearText} {utcOffsetText} ({TimeZone.Id})";
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
        return obj is ZonedYear d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(ZonedYear other)
    {
        return Start.Equals( other.Start );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedYear d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(ZonedYear other)
    {
        return Start.CompareTo( other.Start );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> in the <paramref name="targetTimeZone"/> from this instance.
    /// </summary>
    /// <param name="targetTimeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        return Create( Start.Value, targetTimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> in <see cref="TimeZoneInfo.Utc"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear ToUtcTimeZone()
    {
        return CreateUtc( Start.Value );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> in <see cref="TimeZoneInfo.Local"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> belongs to this year.
    /// </summary>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="dateTime"/> belongs to this year, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDateTime dateTime)
    {
        var start = Start;
        return start.Value.Year == dateTime.ToTimeZone( start.TimeZone ).Year;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="day"/> belongs to this year.
    /// </summary>
    /// <param name="day">Day to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="day"/> belongs to this year, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDay day)
    {
        return Contains( day.Start ) && (ReferenceEquals( TimeZone, day.TimeZone ) || Contains( day.End ));
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="month"/> belongs to this year.
    /// </summary>
    /// <param name="month">Month to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="month"/> belongs to this year, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedMonth month)
    {
        return Contains( month.Start ) && (ReferenceEquals( TimeZone, month.TimeZone ) || Contains( month.End ));
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by calculating the next year.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetNext()
    {
        return AddYears( 1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by calculating the previous year.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetPrevious()
    {
        return AddYears( -1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by adding provided number of years to this instance.
    /// </summary>
    /// <param name="years">Number of years to add.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear AddYears(int years)
    {
        var start = Start;
        var value = start.Value.AddYears( years );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to add.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear Add(Period value)
    {
        var start = Start;
        var dateTime = start.Value.Add( value );
        return Create( dateTime, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by subtracting provided number of years from this instance.
    /// </summary>
    /// <param name="years">Number of years to subtract.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear SubtractYears(int years)
    {
        return AddYears( -years );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to subtract.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear Subtract(Period value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start year.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedYear start, PeriodUnits units)
    {
        return Start.GetPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start year.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedYear start, PeriodUnits units)
    {
        return Start.GetGreedyPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents the specified <paramref name="month"/> of this year.
    /// </summary>
    /// <param name="month">Month to get.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMonth(IsoMonthOfYear month)
    {
        var start = Start;
        return ZonedMonth.Create( start.Year, month, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.January"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetJanuary()
    {
        return GetMonth( IsoMonthOfYear.January );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.February"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetFebruary()
    {
        return GetMonth( IsoMonthOfYear.February );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.March"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMarch()
    {
        return GetMonth( IsoMonthOfYear.March );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.April"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetApril()
    {
        return GetMonth( IsoMonthOfYear.April );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.May"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMay()
    {
        return GetMonth( IsoMonthOfYear.May );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.June"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetJune()
    {
        return GetMonth( IsoMonthOfYear.June );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.July"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetJuly()
    {
        return GetMonth( IsoMonthOfYear.July );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.August"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetAugust()
    {
        return GetMonth( IsoMonthOfYear.August );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.September"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetSeptember()
    {
        return GetMonth( IsoMonthOfYear.September );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.October"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetOctober()
    {
        return GetMonth( IsoMonthOfYear.October );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.November"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetNovember()
    {
        return GetMonth( IsoMonthOfYear.November );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that represents <see cref="IsoMonthOfYear.December"/> of this year.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetDecember()
    {
        return GetMonth( IsoMonthOfYear.December );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents the specified <paramref name="dayOfYear"/> of this year.
    /// </summary>
    /// <param name="dayOfYear">Day of this year to get.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="dayOfYear"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDayOfYear(int dayOfYear)
    {
        var start = Start;
        var value = start.Value.SetDayOfYear( dayOfYear );
        return ZonedDay.Create( value, start.TimeZone );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDay"/> instance that represents the specified <paramref name="dayOfYear"/> of this year.
    /// </summary>
    /// <param name="dayOfYear">Day of this year to get.</param>
    /// <returns>New <see cref="ZonedDay"/> instance or null when <paramref name="dayOfYear"/> is not valid.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay? TryGetDayOfYear(int dayOfYear)
    {
        if ( dayOfYear <= 0 )
            return null;

        var dayCount = DayCount;

        if ( dayOfYear > dayCount )
            return null;

        var start = Start;
        var value = start.Value.SetDayOfYear( dayOfYear );
        return ZonedDay.Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance that represents the specified <paramref name="weekOfYear"/> of this year.
    /// </summary>
    /// <param name="weekOfYear">Week of this year to get.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="weekOfYear"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetWeekOfYear(int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.Create( Year, weekOfYear, TimeZone, weekStart );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance
    /// that represents the specified <paramref name="weekOfYear"/> of this year.
    /// </summary>
    /// <param name="weekOfYear">Week of this year to get.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance or null when <paramref name="weekOfYear"/> is not valid.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek? TryGetWeekOfYear(int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.TryCreate( Year, weekOfYear, TimeZone, weekStart );
    }

    /// <summary>
    /// Calculates the number of weeks in this year.
    /// </summary>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>Number of weeks in this year.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetWeekCount(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( ( int )weekStart, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );
        return WeekCalculator.GetWeekCountInYear( Year, weekStart.ToBcl() );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all weeks of this year in order.
    /// </summary>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<ZonedWeek> GetAllWeeks(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        return GetAllWeeksImpl( weekCount, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all months of this year in order.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<ZonedMonth> GetAllMonths()
    {
        var start = Start;
        var year = start.Year;
        var timeZone = start.TimeZone;

        for ( var month = 1; month <= ChronoConstants.MonthsPerYear; ++month )
            yield return ZonedMonth.Create( year, ( IsoMonthOfYear )month, timeZone );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all days of this year in order.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<ZonedDay> GetAllDays()
    {
        var start = Start;
        var year = start.Year;
        var timeZone = start.TimeZone;

        for ( var day = 1; day <= ChronoConstants.DaysInJanuary; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.January, day ), timeZone );

        var februaryDayCount = DateTime.IsLeapYear( year ) ? ChronoConstants.DaysInLeapFebruary : ChronoConstants.DaysInFebruary;
        for ( var day = 1; day <= februaryDayCount; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.February, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInMarch; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.March, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInApril; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.April, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInMay; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.May, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInJune; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.June, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInJuly; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.July, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInAugust; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.August, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInSeptember; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.September, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInOctober; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.October, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInNovember; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.November, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInDecember; ++day )
            yield return ZonedDay.Create( new DateTime( year, ( int )IsoMonthOfYear.December, day ), timeZone );
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
    /// Creates a new <see cref="ZonedYear"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear operator +(ZonedYear a, Period b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear operator -(ZonedYear a, Period b)
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
    public static bool operator ==(ZonedYear a, ZonedYear b)
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
    public static bool operator !=(ZonedYear a, ZonedYear b)
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
    public static bool operator >(ZonedYear a, ZonedYear b)
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
    public static bool operator <=(ZonedYear a, ZonedYear b)
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
    public static bool operator <(ZonedYear a, ZonedYear b)
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
    public static bool operator >=(ZonedYear a, ZonedYear b)
    {
        return a.CompareTo( b ) >= 0;
    }

    private IEnumerable<ZonedWeek> GetAllWeeksImpl(int weekCount, IsoDayOfWeek weekStart)
    {
        var start = Start;
        var timeZone = start.TimeZone;
        var bclWeekStart = weekStart.ToBcl();
        var startOfWeek = WeekCalculator.GetDayInFirstWeekOfYear( start.Year, bclWeekStart ).GetStartOfWeek( bclWeekStart );

        yield return ZonedWeek.Create( startOfWeek, timeZone, weekStart );

        for ( var week = 2; week <= weekCount; ++week )
        {
            startOfWeek = startOfWeek.AddTicks( ChronoConstants.TicksPerStandardWeek );
            yield return ZonedWeek.Create( startOfWeek, timeZone, weekStart );
        }
    }
}
