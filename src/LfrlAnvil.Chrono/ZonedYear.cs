using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

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

    public ZonedDateTime Start { get; }
    public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfYear() );
    public int Year => Start.Year;
    public bool IsLeap => DateTime.IsLeapYear( Year );
    public int DayCount => IsLeap ? ChronoConstants.DaysInLeapYear : ChronoConstants.DaysInYear;
    public TimeZoneInfo TimeZone => Start.TimeZone;
    public Duration Duration => _duration ?? Duration.FromHours( ChronoConstants.HoursPerStandardDay * ChronoConstants.DaysInYear );
    public bool IsUtc => Start.IsUtc;
    public bool IsLocal => Start.IsLocal;

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(ZonedDateTime dateTime)
    {
        return Create( dateTime.Value, dateTime.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(ZonedDay day)
    {
        return Create( day.Start.Value, day.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(ZonedMonth month)
    {
        return Create( month.Start.Value, month.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear Create(int year, TimeZoneInfo timeZone)
    {
        return Create( new DateTime( year, (int)IsoMonthOfYear.January, 1 ), timeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateUtc(Timestamp timestamp)
    {
        return CreateUtc( timestamp.UtcValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateUtc(DateTime utcDateTime)
    {
        var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfYear() );
        var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfYear() );
        return new ZonedYear( start, end, end.GetDurationOffset( start ).AddTicks( 1 ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateUtc(int year)
    {
        return CreateUtc( new DateTime( year, (int)IsoMonthOfYear.January, 1 ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateLocal(DateTime localDateTime)
    {
        return Create( localDateTime, TimeZoneInfo.Local );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear CreateLocal(int year)
    {
        return Create( new DateTime( year, (int)IsoMonthOfYear.January, 1 ), TimeZoneInfo.Local );
    }

    [Pure]
    public override string ToString()
    {
        var start = Start;
        var timeZone = start.TimeZone;

        var yearText = TextFormatting.StringifyYear( start.Value.Year );
        var utcOffsetText = TextFormatting.StringifyOffset( new Duration( timeZone.BaseUtcOffset ) );

        return $"{yearText} {utcOffsetText} ({TimeZone.Id})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Start.Timestamp ).Add( End.Timestamp ).Add( TimeZone.Id ).Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ZonedYear d && Equals( d );
    }

    [Pure]
    public bool Equals(ZonedYear other)
    {
        return Start.Equals( other.Start );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedYear d ? CompareTo( d ) : 1;
    }

    [Pure]
    public int CompareTo(ZonedYear other)
    {
        return Start.CompareTo( other.Start );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        return Create( Start.Value, targetTimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear ToUtcTimeZone()
    {
        return CreateUtc( Start.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDateTime dateTime)
    {
        var start = Start;
        return start.Value.Year == dateTime.ToTimeZone( start.TimeZone ).Year;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDay day)
    {
        return Contains( day.Start ) && (ReferenceEquals( TimeZone, day.TimeZone ) || Contains( day.End ));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedMonth month)
    {
        return Contains( month.Start ) && (ReferenceEquals( TimeZone, month.TimeZone ) || Contains( month.End ));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetNext()
    {
        return AddYears( 1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetPrevious()
    {
        return AddYears( -1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear AddYears(int years)
    {
        var start = Start;
        var value = start.Value.AddYears( years );
        return Create( value, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear Add(Period value)
    {
        var start = Start;
        var dateTime = start.Value.Add( value );
        return Create( dateTime, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear SubtractYears(int years)
    {
        return AddYears( -years );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear Subtract(Period value)
    {
        return Add( -value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedYear start, PeriodUnits units)
    {
        return Start.GetPeriodOffset( start.Start, units );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedYear start, PeriodUnits units)
    {
        return Start.GetGreedyPeriodOffset( start.Start, units );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMonth(IsoMonthOfYear month)
    {
        var start = Start;
        return ZonedMonth.Create( start.Year, month, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetJanuary()
    {
        return GetMonth( IsoMonthOfYear.January );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetFebruary()
    {
        return GetMonth( IsoMonthOfYear.February );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMarch()
    {
        return GetMonth( IsoMonthOfYear.March );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetApril()
    {
        return GetMonth( IsoMonthOfYear.April );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMay()
    {
        return GetMonth( IsoMonthOfYear.May );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetJune()
    {
        return GetMonth( IsoMonthOfYear.June );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetJuly()
    {
        return GetMonth( IsoMonthOfYear.July );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetAugust()
    {
        return GetMonth( IsoMonthOfYear.August );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetSeptember()
    {
        return GetMonth( IsoMonthOfYear.September );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetOctober()
    {
        return GetMonth( IsoMonthOfYear.October );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetNovember()
    {
        return GetMonth( IsoMonthOfYear.November );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetDecember()
    {
        return GetMonth( IsoMonthOfYear.December );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDayOfYear(int dayOfYear)
    {
        var start = Start;
        var value = start.Value.SetDayOfYear( dayOfYear );
        return ZonedDay.Create( value, start.TimeZone );
    }

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

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetWeekOfYear(int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.Create( Year, weekOfYear, TimeZone, weekStart );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek? TryGetWeekOfYear(int weekOfYear, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.TryCreate( Year, weekOfYear, TimeZone, weekStart );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetWeekCount(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( (int)weekStart, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( weekStart ) );
        return WeekCalculator.GetWeekCountInYear( Year, weekStart.ToBcl() );
    }

    [Pure]
    public IEnumerable<ZonedWeek> GetAllWeeks(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        return GetAllWeeksImpl( weekCount, weekStart );
    }

    [Pure]
    public IEnumerable<ZonedMonth> GetAllMonths()
    {
        var start = Start;
        var year = start.Year;
        var timeZone = start.TimeZone;

        for ( var month = 1; month <= ChronoConstants.MonthsPerYear; ++month )
            yield return ZonedMonth.Create( year, (IsoMonthOfYear)month, timeZone );
    }

    [Pure]
    public IEnumerable<ZonedDay> GetAllDays()
    {
        var start = Start;
        var year = start.Year;
        var timeZone = start.TimeZone;

        for ( var day = 1; day <= ChronoConstants.DaysInJanuary; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.January, day ), timeZone );

        var februaryDayCount = DateTime.IsLeapYear( year ) ? ChronoConstants.DaysInLeapFebruary : ChronoConstants.DaysInFebruary;
        for ( var day = 1; day <= februaryDayCount; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.February, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInMarch; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.March, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInApril; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.April, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInMay; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.May, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInJune; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.June, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInJuly; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.July, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInAugust; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.August, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInSeptember; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.September, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInOctober; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.October, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInNovember; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.November, day ), timeZone );

        for ( var day = 1; day <= ChronoConstants.DaysInDecember; ++day )
            yield return ZonedDay.Create( new DateTime( year, (int)IsoMonthOfYear.December, day ), timeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Bounds<ZonedDateTime> ToBounds()
    {
        return Bounds.Create( Start, End );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public BoundsRange<ZonedDateTime> ToCheckedBounds()
    {
        return ZonedDateTimeBounds.CreateChecked( Start, End );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear operator +(ZonedYear a, Period b)
    {
        return a.Add( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedYear operator -(ZonedYear a, Period b)
    {
        return a.Subtract( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(ZonedYear a, ZonedYear b)
    {
        return a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(ZonedYear a, ZonedYear b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(ZonedYear a, ZonedYear b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(ZonedYear a, ZonedYear b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(ZonedYear a, ZonedYear b)
    {
        return a.CompareTo( b ) < 0;
    }

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
