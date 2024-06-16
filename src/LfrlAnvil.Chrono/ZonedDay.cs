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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Exceptions;
using LfrlAnvil.Chrono.Extensions;
using LfrlAnvil.Chrono.Internal;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a day with time zone.
/// </summary>
public readonly struct ZonedDay : IEquatable<ZonedDay>, IComparable<ZonedDay>, IComparable
{
    private readonly ZonedDateTime? _end;
    private readonly Duration? _duration;

    private ZonedDay(ZonedDateTime start, ZonedDateTime end, Duration duration)
    {
        Start = start;
        _end = end;
        _duration = duration;
    }

    /// <summary>
    /// Start of this day.
    /// </summary>
    public ZonedDateTime Start { get; }

    /// <summary>
    /// End of this day.
    /// </summary>
    public ZonedDateTime End => _end ?? ZonedDateTime.CreateUtc( DateTime.UnixEpoch.GetEndOfDay() );

    /// <summary>
    /// Year component.
    /// </summary>
    public int Year => Start.Year;

    /// <summary>
    /// Month component.
    /// </summary>
    public IsoMonthOfYear Month => Start.Month;

    /// <summary>
    /// Day of month component.
    /// </summary>
    public int DayOfMonth => Start.DayOfMonth;

    /// <summary>
    /// Day of year component.
    /// </summary>
    public int DayOfYear => Start.DayOfYear;

    /// <summary>
    /// Day of week component.
    /// </summary>
    public IsoDayOfWeek DayOfWeek => Start.DayOfWeek;

    /// <summary>
    /// Time zone of this day.
    /// </summary>
    public TimeZoneInfo TimeZone => Start.TimeZone;

    /// <summary>
    /// <see cref="Chrono.Duration"/> of this day.
    /// </summary>
    public Duration Duration => _duration ?? Duration.FromHours( ChronoConstants.HoursPerStandardDay );

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is UTC.
    /// </summary>
    public bool IsUtc => Start.IsUtc;

    /// <summary>
    /// Checks whether or not the <see cref="TimeZone"/> is local.
    /// </summary>
    public bool IsLocal => Start.IsLocal;

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <param name="timeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    public static ZonedDay Create(DateTime dateTime, TimeZoneInfo timeZone)
    {
        var kind = timeZone.GetDateTimeKind();
        dateTime = DateTime.SpecifyKind( dateTime, kind );

        var (start, startDurationOffset) = dateTime.GetStartOfDay().CreateIntervalStart( timeZone );
        var (end, endDurationOffset) = dateTime.GetEndOfDay().CreateIntervalEnd( timeZone );
        var duration = end.GetDurationOffset( start ).Add( startDurationOffset ).Add( endDurationOffset ).AddTicks( 1 );

        return new ZonedDay( start, end, duration );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance.
    /// </summary>
    /// <param name="dateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDay Create(ZonedDateTime dateTime)
    {
        return Create( dateTime.Value, dateTime.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="timestamp">Timestamp contained by the result.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDay CreateUtc(Timestamp timestamp)
    {
        return CreateUtc( timestamp.UtcValue );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance in <see cref="TimeZoneInfo.Utc"/> time zone.
    /// </summary>
    /// <param name="utcDateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDay CreateUtc(DateTime utcDateTime)
    {
        var start = ZonedDateTime.CreateUtc( utcDateTime.GetStartOfDay() );
        var end = ZonedDateTime.CreateUtc( utcDateTime.GetEndOfDay() );
        return new ZonedDay( start, end, Duration.FromHours( ChronoConstants.HoursPerStandardDay ) );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance in <see cref="TimeZoneInfo.Local"/> time zone.
    /// </summary>
    /// <param name="localDateTime">Date time contained by the result.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDay CreateLocal(DateTime localDateTime)
    {
        return Create( localDateTime, TimeZoneInfo.Local );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="ZonedDay"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var start = Start;
        var startUtcOffset = start.UtcOffset;
        var endUtcOffset = End.UtcOffset;

        var dateText = TextFormatting.StringifyDate( start.Value );
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
        return obj is ZonedDay d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    public bool Equals(ZonedDay other)
    {
        return Start.Equals( other.Start );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is ZonedDay d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(ZonedDay other)
    {
        return Start.CompareTo( other.Start );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> in the <paramref name="targetTimeZone"/> from this instance.
    /// </summary>
    /// <param name="targetTimeZone">Target time zone.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay ToTimeZone(TimeZoneInfo targetTimeZone)
    {
        return Create( Start.Value, targetTimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> in <see cref="TimeZoneInfo.Utc"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay ToUtcTimeZone()
    {
        return CreateUtc( Start.Value );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> in <see cref="TimeZoneInfo.Local"/> time zone from this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay ToLocalTimeZone()
    {
        return ToTimeZone( TimeZoneInfo.Local );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="dateTime"/> belongs to this day.
    /// </summary>
    /// <param name="dateTime">Date time to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="dateTime"/> belongs to this day, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Contains(ZonedDateTime dateTime)
    {
        var start = Start;
        return start.Value.Date == dateTime.ToTimeZone( start.TimeZone ).Value.Date;
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by calculating the next day.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetNext()
    {
        return AddDays( 1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by calculating the previous day.
    /// </summary>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay GetPrevious()
    {
        return AddDays( -1 );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by adding provided number of days to this instance.
    /// </summary>
    /// <param name="days">Number of days to add.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay AddDays(int days)
    {
        var start = Start;
        var value = start.Value.AddTicks( ChronoConstants.TicksPerStandardDay * days );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by adding <paramref name="value"/> to this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to add.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay Add(Period value)
    {
        var start = Start;
        var dateTime = start.Value.Add( value );
        return Create( dateTime, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by subtracting provided number of days from this instance.
    /// </summary>
    /// <param name="days">Number of days to subtract.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay SubtractDays(int days)
    {
        return AddDays( -days );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by subtracting <paramref name="value"/> from this instance.
    /// </summary>
    /// <param name="value"><see cref="Period"/> to subtract.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay Subtract(Period value)
    {
        return Add( -value );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start day.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetPeriodOffset(ZonedDay start, PeriodUnits units)
    {
        return Start.GetPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating a difference between this instance and
    /// the <paramref name="start"/> instance, where this instance is treated as the end of the range,
    /// using the specified <paramref name="units"/>.
    /// </summary>
    /// <param name="start">Start day.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to include in the calculated difference.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    /// <remarks>Greedy <see cref="Period"/> may contain components with negative values.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period GetGreedyPeriodOffset(ZonedDay start, PeriodUnits units)
    {
        return Start.GetGreedyPeriodOffset( start.Start, units );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by setting the <see cref="ZonedDay.Year"/> component in this instance.
    /// </summary>
    /// <param name="year">Year to set.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="year"/> is not valid.</exception>
    /// <remarks>
    /// Result may end up with modified components other than the year,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay SetYear(int year)
    {
        var start = Start;
        var value = start.Value.SetYear( year );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by setting the <see cref="ZonedDay.Month"/> component in this instance.
    /// </summary>
    /// <param name="month">Month to set.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    /// <remarks>
    /// Result may end up with modified components other than the month,
    /// if it lands in the range of invalid values of this instance's <see cref="TimeZone"/>.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay SetMonth(IsoMonthOfYear month)
    {
        var start = Start;
        var value = start.Value.SetMonth( month );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by setting the <see cref="ZonedDay.DayOfMonth"/> component in this instance.
    /// </summary>
    /// <param name="day">Day of month to set.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="day"/> is not valid for the current month.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay SetDayOfMonth(int day)
    {
        var start = Start;
        var value = start.Value.SetDayOfMonth( day );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by setting the <see cref="ZonedDay.DayOfYear"/> component in this instance.
    /// </summary>
    /// <param name="day">Day of year to set.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="day"/> is not valid for the current year.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDay SetDayOfYear(int day)
    {
        var start = Start;
        var value = start.Value.SetDayOfYear( day );
        return Create( value, start.TimeZone );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.TimeOfDay"/>
    /// component in this instance's <see cref="Start"/> date time.
    /// </summary>
    /// <param name="timeOfDay">Time of day to set.</param>
    /// <returns>New <see cref="ZonedDateTime"/> instance.</returns>
    /// <exception cref="InvalidZonedDateTimeException">When result is not valid in this instance's <see cref="TimeZone"/>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime GetDateTime(TimeOfDay timeOfDay)
    {
        return Start.SetTimeOfDay( timeOfDay );
    }

    /// <summary>
    /// Attempts to create a new <see cref="ZonedDateTime"/> instance by setting the <see cref="ZonedDateTime.TimeOfDay"/>
    /// component in this instance's <see cref="Start"/> date time.
    /// </summary>
    /// <param name="timeOfDay">Time of day to set.</param>
    /// <returns>
    /// New <see cref="ZonedDateTime"/> instance or null when result is not valid in this instance's <see cref="TimeZone"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedDateTime? TryGetDateTime(TimeOfDay timeOfDay)
    {
        return Start.TrySetTimeOfDay( timeOfDay );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedWeek"/> instance that contains this instance.
    /// </summary>
    /// <param name="weekStart">First day of the week.</param>
    /// <returns>New <see cref="ZonedWeek"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedWeek GetWeek(IsoDayOfWeek weekStart = IsoDayOfWeek.Monday)
    {
        return ZonedWeek.Create( this, weekStart );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedMonth"/> instance that contains this instance.
    /// </summary>
    /// <returns>New <see cref="ZonedMonth"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ZonedMonth GetMonth()
    {
        return ZonedMonth.Create( this );
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
    /// Attempts to find the range of invalid date times in this instance, according to its <see cref="TimeZone"/>.
    /// </summary>
    /// <returns>
    /// New <see cref="Bounds{T}"/> instance that represents the range of invalid date times
    /// or null when this instance does not contain invalid date times.
    /// </returns>
    [Pure]
    public Bounds<DateTime>? GetIntersectingInvalidityRange()
    {
        var start = Start;
        var timeZone = start.TimeZone;
        var startValue = start.Value;

        var activeRule = timeZone.GetActiveAdjustmentRule( startValue );
        if ( activeRule is null || activeRule.DaylightDelta == TimeSpan.Zero )
            return null;

        var transitionTime = activeRule.GetTransitionTimeWithInvalidity();
        var transitionStart = transitionTime.ToDateTime( startValue.Year );
        if ( transitionStart > startValue.GetEndOfDay() )
            return null;

        var transitionEnd = transitionStart.Add( activeRule.DaylightDelta.Abs() ).AddTicks( -1 );
        if ( transitionEnd < startValue.GetStartOfDay() )
            return null;

        // NOTE: sanity check for adjustment rules internally marked as [Start/End]DateMarkerFor[End/Beginning]OfYear
        if ( ! timeZone.IsInvalidTime( transitionStart ) )
            return null;

        return Bounds.Create( transitionStart, transitionEnd );
    }

    /// <summary>
    /// Attempts to find the range of ambiguous date times in this instance, according to its <see cref="TimeZone"/>.
    /// </summary>
    /// <returns>
    /// New <see cref="Bounds{T}"/> instance that represents the range of ambiguous date times
    /// or null when this instance does not contain invalid date times.
    /// </returns>
    [Pure]
    public Bounds<DateTime>? GetIntersectingAmbiguityRange()
    {
        var start = Start;
        var timeZone = start.TimeZone;
        var startValue = start.Value;

        var activeRule = timeZone.GetActiveAdjustmentRule( startValue );
        if ( activeRule is null || activeRule.DaylightDelta == TimeSpan.Zero )
            return null;

        var transitionTime = activeRule.GetTransitionTimeWithAmbiguity();
        var transitionEnd = transitionTime.ToDateTime( startValue.Year ).AddTicks( -1 );
        if ( transitionEnd < startValue.GetStartOfDay() )
            return null;

        var transitionStart = transitionEnd.Add( -activeRule.DaylightDelta.Abs() ).AddTicks( 1 );
        if ( transitionStart > startValue.GetEndOfDay() )
            return null;

        // NOTE: sanity check for adjustment rules internally marked as [Start/End]DateMarkerFor[End/Beginning]OfYear
        if ( ! timeZone.IsAmbiguousTime( transitionStart ) )
            return null;

        return Bounds.Create( transitionStart, transitionEnd );
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
    /// Coverts the provided <paramref name="source"/> to <see cref="ZonedDateTime"/>.
    /// </summary>
    /// <param name="source">Value to convert.</param>
    /// <returns><see cref="Start"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator ZonedDateTime(ZonedDay source)
    {
        return source.Start;
    }

    /// <summary>
    /// Coverts the provided <paramref name="source"/> to <see cref="DateTime"/>.
    /// </summary>
    /// <param name="source">Value to convert.</param>
    /// <returns><see cref="ZonedDateTime.Value"/> of <see cref="Start"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator DateTime(ZonedDay source)
    {
        return source.Start.Value;
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDay operator +(ZonedDay a, Period b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="ZonedDay"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="ZonedDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ZonedDay operator -(ZonedDay a, Period b)
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
    public static bool operator ==(ZonedDay a, ZonedDay b)
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
    public static bool operator !=(ZonedDay a, ZonedDay b)
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
    public static bool operator >(ZonedDay a, ZonedDay b)
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
    public static bool operator <=(ZonedDay a, ZonedDay b)
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
    public static bool operator <(ZonedDay a, ZonedDay b)
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
    public static bool operator >=(ZonedDay a, ZonedDay b)
    {
        return a.CompareTo( b ) >= 0;
    }
}
