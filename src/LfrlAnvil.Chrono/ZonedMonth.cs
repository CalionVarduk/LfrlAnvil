using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;

namespace LfrlAnvil.Chrono;

public readonly struct ZonedMonth : IEquatable<ZonedMonth>, IComparable<ZonedMonth>, IComparable
{
    private readonly ZonedDateTime? _end;
    private readonly Duration? _duration;

    private ZonedMonth(ZonedDateTime start, ZonedDateTime end, Duration duration)
    {
        Start = start;
        _end = end;
        _duration = duration;
    }

    public ZonedDateTime Start { get; }
    public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfMonth() );
    public int Year => Start.Year;
    public IsoMonthOfYear Month => Start.Month;
    public int DayCount => DateTime.DaysInMonth( Year, (int)Month );
    public TimeZoneInfo TimeZone => Start.TimeZone;
    public Duration Duration => _duration ?? Duration.FromHours( ChronoConstants.HoursPerDay * ChronoConstants.DaysInJanuary );
    public bool IsUtc => Start.IsUtc;
    public bool IsLocal => Start.IsLocal;

    [Pure]
    public static ZonedMonth Create(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var kind = timeZone.GetDateTimeKind();
        dateTime = DateTime.SpecifyKind( dateTime, kind );

        var (start, startDurationOffset) = dateTime.GetStartOfMonth().CreateIntervalStart( timeZone );
        var (end, endDurationOffset) = dateTime.GetEndOfMonth().CreateIntervalEnd( timeZone );
        var duration = end.GetDurationOffset( start ).Add( startDurationOffset ).Add( endDurationOffset ).AddTicks( 1 );

        return new ZonedMonth( start, end, duration );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth Create(ZonedDateTime dateTime)
    {
        return Create( dateTime.Value, dateTime.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth Create(ZonedDay day)
    {
        return Create( day.Start.Value, day.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth Create(int year, IsoMonthOfYear month, TimeZoneInfo timeZone)
    {
        return Create( new DateTime( year, (int)month, 1 ), timeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateUtc(Timestamp timestamp)
    {
        return CreateUtc( timestamp.UtcValue );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateUtc(DateTime utcDateTime)
    {
        var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfMonth() );
        var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfMonth() );
        return new ZonedMonth( start, end, end.GetDurationOffset( start ).AddTicks( 1 ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateUtc(int year, IsoMonthOfYear month)
    {
        return CreateUtc( new DateTime( year, (int)month, 1 ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateLocal(DateTime localDateTime)
    {
        return Create( localDateTime, TimeZoneInfo.Local );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateLocal(int year, IsoMonthOfYear month)
    {
        return Create( new DateTime( year, (int)month, 1 ), TimeZoneInfo.Local );
    }

    [Pure]
    public override string ToString()
    {
        var start = Start;
        var startUtcOffset = start.UtcOffset;
        var endUtcOffset = End.UtcOffset;

        var dateText = TextFormatting.StringifyYearAndMonth( start.Value );
        var utcOffsetText = TextFormatting.StringifyOffset( start.UtcOffset );

        if ( startUtcOffset == endUtcOffset )
            return $"{dateText} {utcOffsetText} ({TimeZone.Id})";

        var endUtcOffsetText = TextFormatting.StringifyOffset( endUtcOffset );
        return $"{dateText} {utcOffsetText} {endUtcOffsetText} ({TimeZone.Id})";
    }

    [Pure]
    public override int GetHashCode()
    {
        return Hash.Default.Add( Start.Timestamp ).Add( End.Timestamp ).Add( TimeZone.Id ).Value;
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is ZonedMonth d && Equals( d );
    }

    [Pure]
    public bool Equals(ZonedMonth other)
    {
        return Start.Equals( other.Start );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedMonth d ? CompareTo( d ) : 1;
    }

    [Pure]
    public int CompareTo(ZonedMonth other)
    {
        return Start.CompareTo( other.Start );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        return Create( Start.Value, targetTimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth ToUtcTimeZone()
    {
        return CreateUtc( Start.Value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDateTime dateTime)
    {
        var start = Start;
        var startValue = start.Value;
        var convertedDateTime = dateTime.ToTimeZone( start.TimeZone ).Value;
        return startValue.Year == convertedDateTime.Year && startValue.Month == convertedDateTime.Month;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDay day)
    {
        return Contains( day.Start ) && (ReferenceEquals( TimeZone, day.TimeZone ) || Contains( day.End ));
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetNext()
    {
        return AddMonths( 1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetPrevious()
    {
        return AddMonths( -1 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth AddMonths(int months)
    {
        var start = Start;
        var value = start.Value.AddMonths( months );
        return Create( value, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth Add(Period value)
    {
        var start = Start;
        var dateTime = start.Value.Add( value );
        return Create( dateTime, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth SubtractMonths(int months)
    {
        return AddMonths( -months );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth Subtract(Period value)
    {
        return Add( -value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedMonth start, PeriodUnits units)
    {
        return Start.GetPeriodOffset( start.Start, units );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedMonth start, PeriodUnits units)
    {
        return Start.GetGreedyPeriodOffset( start.Start, units );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth SetYear(int year)
    {
        var start = Start;
        var value = start.Value.SetYear( year );
        return Create( value, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth SetMonth(IsoMonthOfYear month)
    {
        var start = Start;
        var value = start.Value.SetMonth( month );
        return Create( value, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDayOfMonth(int dayOfMonth)
    {
        var start = Start;
        var value = start.Value.SetDayOfMonth( dayOfMonth );
        return ZonedDay.Create( value, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay? TryGetDayOfMonth(int dayOfMonth)
    {
        if ( dayOfMonth <= 0 )
            return null;

        var dayCount = DayCount;

        if ( dayOfMonth > dayCount )
            return null;

        var start = Start;
        var value = start.Value.SetDayOfMonth( dayOfMonth );
        return ZonedDay.Create( value, start.TimeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetYear()
    {
        return ZonedYear.Create( this );
    }

    [Pure]
    public ZonedWeek GetWeekOfMonth(int weekOfMonth, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        Ensure.IsInRange( weekOfMonth, 1, weekCount, nameof( weekOfMonth ) );
        return GetWeekOfMonthUnsafe( weekOfMonth, weekStart );
    }

    [Pure]
    public ZonedWeek? TryGetWeekOfMonth(int weekOfMonth, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        if ( weekOfMonth <= 0 || weekOfMonth > weekCount )
            return null;

        return GetWeekOfMonthUnsafe( weekOfMonth, weekStart );
    }

    [Pure]
    public IEnumerable<ZonedDay> GetAllDays()
    {
        var dayCount = DayCount;
        var start = Start;
        var year = start.Year;
        var month = (int)start.Month;
        var timeZone = start.TimeZone;

        for ( var day = 1; day <= dayCount; ++day )
            yield return ZonedDay.Create( new DateTime( year, month, day ), timeZone );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetWeekCount(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( (int)weekStart, (int)IsoDayOfWeek.Monday, (int)IsoDayOfWeek.Sunday, nameof( weekStart ) );
        return WeekCalculator.GetWeekCountInMonth( Start.Value, End.Value, weekStart.ToBcl() );
    }

    [Pure]
    public IEnumerable<ZonedWeek> GetAllWeeks(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        return GetAllWeeksImpl( weekCount, weekStart );
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
    public static ZonedMonth operator +(ZonedMonth a, Period b)
    {
        return a.Add( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth operator -(ZonedMonth a, Period b)
    {
        return a.Subtract( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(ZonedMonth a, ZonedMonth b)
    {
        return a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(ZonedMonth a, ZonedMonth b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(ZonedMonth a, ZonedMonth b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(ZonedMonth a, ZonedMonth b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(ZonedMonth a, ZonedMonth b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(ZonedMonth a, ZonedMonth b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ZonedWeek GetWeekOfMonthUnsafe(int weekOfMonth, IsoDayOfWeek weekStart)
    {
        var startOfFirstWeek = Start.Value.GetStartOfWeek( weekStart.ToBcl() );
        var startOfTargetWeek = startOfFirstWeek.AddTicks( ChronoConstants.TicksPerWeek * (weekOfMonth - 1) );
        return ZonedWeek.Create( startOfTargetWeek, TimeZone, weekStart );
    }

    [Pure]
    private IEnumerable<ZonedWeek> GetAllWeeksImpl(int weekCount, IsoDayOfWeek weekStart)
    {
        var start = Start;
        var timeZone = start.TimeZone;
        var startOfWeek = start.Value.GetStartOfWeek( weekStart.ToBcl() );

        yield return ZonedWeek.Create( startOfWeek, timeZone, weekStart );

        for ( var week = 2; week <= weekCount; ++week )
        {
            startOfWeek = startOfWeek.AddTicks( ChronoConstants.TicksPerWeek );
            yield return ZonedWeek.Create( startOfWeek, timeZone, weekStart );
        }
    }
}
