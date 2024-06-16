// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a month with time zone.
/// </summary>
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

    /// <summary>
    /// Start of this month.
    /// </summary>
    public ZonedDateTime Start { get; }

    /// <summary>
    /// End of this month.
    /// </summary>
    public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfMonth() );

    /// <summary>
    /// Year component.
    /// </summary>
    public int Year => Start.Year;

    /// <summary>
    /// <see cref="IsoMonthOfYear"/> descriptor of this month.
    /// </summary>
    public IsoMonthOfYear Month => Start.Month;

    /// <summary>
    /// Number of days in this month.
    /// </summary>
    public int DayCount => DateTime.DaysInMonth( Year, ( int )Month );

    /// <summary>
    /// Time zone of this month.
    /// </summary>
    public TimeZoneInfo TimeZone => Start.TimeZone;

    /// <summary>
    /// <see cref="Chrono.Duration"/> of this month.
    /// </summary>
    public Duration Duration => _duration ?? Duration.FromHours( ChronoConstants.HoursPerStandardDay * ChronoConstants.DaysInJanuary );

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is UTC.
    /// </summary>
    public bool IsUtc => Start.IsUtc;

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is local.
    /// </summary>
    public bool IsLocal => Start.IsLocal;

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth Create(ZonedDateTime dateTime)
    {
        return Create( dateTime.Value, dateTime.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance.
    /// </summary>
    /// <param name="day">Day contained by the result.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth Create(ZonedDay day)
    {
        return Create( day.Start.Value, day.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance.
    /// </summary>
    /// <param name="year">Year component.</param>
    /// <param name="month">Month component.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth Create(int year, IsoMonthOfYear month, TimeZoneInfo timeZone)
    {
        return Create( new DateTime( year, ( int )month, 1 ), timeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="timestamp">Timestamp contained by the result.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateUtc(Timestamp timestamp)
    {
        return CreateUtc( timestamp.UtcValue );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="utcDateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateUtc(DateTime utcDateTime)
    {
        var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfMonth() );
        var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfMonth() );
        return new ZonedMonth( start, end, end.GetDurationOffset( start ).AddTicks( 1 ) );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="year">Year component.</param>
    /// <param name="month">Month component.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateUtc(int year, IsoMonthOfYear month)
    {
        return CreateUtc( new DateTime( year, ( int )month, 1 ) );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="localDateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateLocal(DateTime localDateTime)
    {
        return Create( localDateTime, TimeZoneInfo.Local );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="year">Year component.</param>
    /// <param name="month">Month component.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth CreateLocal(int year, IsoMonthOfYear month)
    {
        return Create( new DateTime( year, ( int )month, 1 ), TimeZoneInfo.Local );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ZonedMonth"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
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
        return obj is ZonedMonth d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(ZonedMonth other)
    {
        return Start.Equals( other.Start );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedMonth d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(ZonedMonth other)
    {
        return Start.CompareTo( other.Start );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> in the <paramref name="targetTimeZone"/> from this instance.
    /// </summary>
    /// <param name="targetTimeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        return Create( Start.Value, targetTimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> in <see cref="TimeZoneInfo.Utc"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth ToUtcTimeZone()
    {
        return CreateUtc( Start.Value );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> in <see cref="TimeZoneInfo.Local"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> belongs to this month.
    /// </summary>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="dateTime"/> belongs to this month, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDateTime dateTime)
    {
        var start = Start;
        var startValue = start.Value;
        var convertedDateTime = dateTime.ToTimeZone( start.TimeZone ).Value;
        return startValue.Year == convertedDateTime.Year && startValue.Month == convertedDateTime.Month;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="day"/> belongs to this month.
    /// </summary>
    /// <param name="day">Day to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="day"/> belongs to this month, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDay day)
    {
        return Contains( day.Start ) && (ReferenceEquals( TimeZone, day.TimeZone ) || Contains( day.End ));
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by calculating the next month.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetNext()
    {
        return AddMonths( 1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by calculating the previous month.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetPrevious()
    {
        return AddMonths( -1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by adding provided number of months to this instance.
    /// </summary>
    /// <param name="months">Number of months to add.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth AddMonths(int months)
    {
        var start = Start;
        var value = start.Value.AddMonths( months );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to add.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth Add(Period value)
    {
        var start = Start;
        var dateTime = start.Value.Add( value );
        return Create( dateTime, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by subtracting provided number of months from this instance.
    /// </summary>
    /// <param name="months">Number of months to subtract.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth SubtractMonths(int months)
    {
        return AddMonths( -months );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to subtract.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth Subtract(Period value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start month.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedMonth start, PeriodUnits units)
    {
        return Start.GetPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start month.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedMonth start, PeriodUnits units)
    {
        return Start.GetGreedyPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by setting the <see cref="ZonedMonth.Year"/> component in this instance.
    /// </summary>
    /// <param name="year">Year to set.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth SetYear(int year)
    {
        var start = Start;
        var value = start.Value.SetYear( year );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by setting the <see cref="ZonedMonth.Month"/> component in this instance.
    /// </summary>
    /// <param name="month">Month to set.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth SetMonth(IsoMonthOfYear month)
    {
        var start = Start;
        var value = start.Value.SetMonth( month );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance that represents the specified <paramref name="dayOfMonth"/> of this month.
    /// </summary>
    /// <param name="dayOfMonth">Day of this month to get.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="dayOfMonth"/> is not valid.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetDayOfMonth(int dayOfMonth)
    {
        var start = Start;
        var value = start.Value.SetDayOfMonth( dayOfMonth );
        return ZonedDay.Create( value, start.TimeZone );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDay"/> instance that represents the specified <paramref name="dayOfMonth"/> of this month.
    /// </summary>
    /// <param name="dayOfMonth">Day of this month to get.</param>
    /// <returns>New <see cref="ZonedDay"/> instance or null when <paramref name="dayOfMonth"/> is not valid.</returns>
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

    /// <summary>
    /// Creates a new <see cref="ZonedYear"/> instance that contains this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedYear"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedYear GetYear()
    {
        return ZonedYear.Create( this );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance that represents the specified <paramref name="weekOfMonth"/> of this month.
    /// </summary>
    /// <param name="weekOfMonth">Week of this month to get.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="weekOfMonth"/> is not valid.</exception>
    [Pure]
    public ZonedWeek GetWeekOfMonth(int weekOfMonth, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        Ensure.IsInRange( weekOfMonth, 1, weekCount );
        return GetWeekOfMonthUnsafe( weekOfMonth, weekStart );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedWeek"/> instance
    /// that represents the specified <paramref name="weekOfMonth"/> of this month.
    /// </summary>
    /// <param name="weekOfMonth">Week of this month to get.</param>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance or null when <paramref name="weekOfMonth"/> is not valid.</returns>
    [Pure]
    public ZonedWeek? TryGetWeekOfMonth(int weekOfMonth, IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        var weekCount = GetWeekCount( weekStart );
        if ( weekOfMonth <= 0 || weekOfMonth > weekCount )
            return null;

        return GetWeekOfMonthUnsafe( weekOfMonth, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all days of this month in order.
    /// </summary>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public IEnumerable<ZonedDay> GetAllDays()
    {
        var dayCount = DayCount;
        var start = Start;
        var year = start.Year;
        var month = ( int )start.Month;
        var timeZone = start.TimeZone;

        for ( var day = 1; day <= dayCount; ++day )
            yield return ZonedDay.Create( new DateTime( year, month, day ), timeZone );
    }

    /// <summary>
    /// Calculates the number of weeks in this month.
    /// </summary>
    /// <param name="weekStart">First day of the week. Equal to <see cref="IsoDayOfWeek.Monday"/> by default.</param>
    /// <returns>Number of weeks in this month.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int GetWeekCount(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        Ensure.IsInRange( ( int )weekStart, ( int )IsoDayOfWeek.Monday, ( int )IsoDayOfWeek.Sunday );
        return WeekCalculator.GetWeekCountInMonth( Start.Value, End.Value, weekStart.ToBcl() );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all weeks of this month in order.
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
    /// Creates a new <see cref="ZonedMonth"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth operator +(ZonedMonth a, Period b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedMonth operator -(ZonedMonth a, Period b)
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
    public static bool operator ==(ZonedMonth a, ZonedMonth b)
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
    public static bool operator !=(ZonedMonth a, ZonedMonth b)
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
    public static bool operator >(ZonedMonth a, ZonedMonth b)
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
    public static bool operator <=(ZonedMonth a, ZonedMonth b)
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
    public static bool operator <(ZonedMonth a, ZonedMonth b)
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
    public static bool operator >=(ZonedMonth a, ZonedMonth b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ZonedWeek GetWeekOfMonthUnsafe(int weekOfMonth, IsoDayOfWeek weekStart)
    {
        var startOfFirstWeek = Start.Value.GetStartOfWeek( weekStart.ToBcl() );
        var startOfTargetWeek = startOfFirstWeek.AddTicks( ChronoConstants.TicksPerStandardWeek * (weekOfMonth - 1) );
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
            startOfWeek = startOfWeek.AddTicks( ChronoConstants.TicksPerStandardWeek );
            yield return ZonedWeek.Create( startOfWeek, timeZone, weekStart );
        }
    }
}
