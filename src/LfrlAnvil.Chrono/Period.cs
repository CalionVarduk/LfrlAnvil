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
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a difference between two timestamps as separate date and/or time components.
/// </summary>
public readonly struct Period : IEquatable<Period>
{
    /// <summary>
    /// Represents an empty <see cref="Period"/>, without any <see cref="ActiveUnits"/>.
    /// </summary>
    public static readonly Period Empty = new Period();

    /// <summary>
    /// Creates a new <see cref="Period"/> instance from date components.
    /// </summary>
    /// <param name="years">Number of years.</param>
    /// <param name="months">Number of months.</param>
    /// <param name="weeks">Number of weeks.</param>
    /// <param name="days">Number of days.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period(int years, int months, int weeks, int days)
        : this( years, months, weeks, days, 0, 0, 0, 0, 0, 0 ) { }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance from time components.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <param name="ticks">Number of ticks.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period(int hours, long minutes, long seconds, long milliseconds, long microseconds, long ticks)
        : this( 0, 0, 0, 0, hours, minutes, seconds, milliseconds, microseconds, ticks ) { }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="years">Number of years.</param>
    /// <param name="months">Number of months.</param>
    /// <param name="weeks">Number of weeks.</param>
    /// <param name="days">Number of days.</param>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <param name="ticks">Number of ticks.</param>
    public Period(
        int years,
        int months,
        int weeks,
        int days,
        int hours,
        long minutes,
        long seconds,
        long milliseconds,
        long microseconds,
        long ticks)
    {
        Years = years;
        Months = months;
        Weeks = weeks;
        Days = days;
        Hours = hours;
        Minutes = minutes;
        Seconds = seconds;
        Milliseconds = milliseconds;
        Microseconds = microseconds;
        Ticks = ticks;
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="timeSpan">Source <see cref="TimeSpan"/>.</param>
    public Period(TimeSpan timeSpan)
        : this(
            0,
            0,
            0,
            ( int )timeSpan.TotalDays,
            timeSpan.Hours,
            timeSpan.Minutes,
            timeSpan.Seconds,
            timeSpan.Milliseconds,
            timeSpan.Microseconds,
            timeSpan.Ticks % ChronoConstants.TicksPerMicrosecond ) { }

    /// <summary>
    /// Number of years.
    /// </summary>
    public int Years { get; }

    /// <summary>
    /// Number of months.
    /// </summary>
    public int Months { get; }

    /// <summary>
    /// Number of weeks.
    /// </summary>
    public int Weeks { get; }

    /// <summary>
    /// Number of days.
    /// </summary>
    public int Days { get; }

    /// <summary>
    /// Number of hours.
    /// </summary>
    public int Hours { get; }

    /// <summary>
    /// Number of minutes.
    /// </summary>
    public long Minutes { get; }

    /// <summary>
    /// Number of seconds.
    /// </summary>
    public long Seconds { get; }

    /// <summary>
    /// Number of milliseconds.
    /// </summary>
    public long Milliseconds { get; }

    /// <summary>
    /// Number of microseconds.
    /// </summary>
    public long Microseconds { get; }

    /// <summary>
    /// Number of ticks.
    /// </summary>
    public long Ticks { get; }

    /// <summary>
    /// Checks which date and time components have values different than <b>0</b> and returns a <see cref="PeriodUnits"/> instance.
    /// </summary>
    public PeriodUnits ActiveUnits =>
        (Years != 0 ? PeriodUnits.Years : PeriodUnits.None)
        | (Months != 0 ? PeriodUnits.Months : PeriodUnits.None)
        | (Weeks != 0 ? PeriodUnits.Weeks : PeriodUnits.None)
        | (Days != 0 ? PeriodUnits.Days : PeriodUnits.None)
        | (Hours != 0 ? PeriodUnits.Hours : PeriodUnits.None)
        | (Minutes != 0 ? PeriodUnits.Minutes : PeriodUnits.None)
        | (Seconds != 0 ? PeriodUnits.Seconds : PeriodUnits.None)
        | (Milliseconds != 0 ? PeriodUnits.Milliseconds : PeriodUnits.None)
        | (Microseconds != 0 ? PeriodUnits.Microseconds : PeriodUnits.None)
        | (Ticks != 0 ? PeriodUnits.Ticks : PeriodUnits.None);

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="ticks">Number of ticks.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromTicks(long ticks)
    {
        return new Period( 0, 0, 0, 0, 0, ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromMicroseconds(long microseconds)
    {
        return new Period( 0, 0, 0, 0, microseconds, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromMilliseconds(long milliseconds)
    {
        return new Period( 0, 0, 0, milliseconds, 0, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromSeconds(long seconds)
    {
        return new Period( 0, 0, seconds, 0, 0, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromMinutes(long minutes)
    {
        return new Period( 0, minutes, 0, 0, 0, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromHours(int hours)
    {
        return new Period( hours, 0, 0, 0, 0, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromDays(int days)
    {
        return new Period( 0, 0, 0, days );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="weeks">Number of weeks.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromWeeks(int weeks)
    {
        return new Period( 0, 0, weeks, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="months">Number of months.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromMonths(int months)
    {
        return new Period( 0, months, 0, 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance.
    /// </summary>
    /// <param name="years">Number of years.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period FromYears(int years)
    {
        return new Period( years, 0, 0, 0 );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Period"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        var texts = new[]
        {
            Years != 0 ? $"{Years} year(s)" : string.Empty,
            Months != 0 ? $"{Months} month(s)" : string.Empty,
            Weeks != 0 ? $"{Weeks} week(s)" : string.Empty,
            Days != 0 ? $"{Days} day(s)" : string.Empty,
            Hours != 0 ? $"{Hours} hour(s)" : string.Empty,
            Minutes != 0 ? $"{Minutes} minute(s)" : string.Empty,
            Seconds != 0 ? $"{Seconds} second(s)" : string.Empty,
            Milliseconds != 0 ? $"{Milliseconds} millisecond(s)" : string.Empty,
            Microseconds != 0 ? $"{Microseconds} microsecond(s)" : string.Empty,
            Ticks != 0 ? $"{Ticks} tick(s)" : string.Empty
        };

        var result = string.Join( ", ", texts.Where( static t => t.Length > 0 ) );
        return result.Length != 0 ? result : "0 day(s)";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Hash.Default
            .Add( Years )
            .Add( Months )
            .Add( Weeks )
            .Add( Days )
            .Add( Hours )
            .Add( Minutes )
            .Add( Seconds )
            .Add( Milliseconds )
            .Add( Microseconds )
            .Add( Ticks )
            .Value;
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is Period p && Equals( p );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Period other)
    {
        return Years.Equals( other.Years )
            && Months.Equals( other.Months )
            && Weeks.Equals( other.Weeks )
            && Days.Equals( other.Days )
            && Hours.Equals( other.Hours )
            && Minutes.Equals( other.Minutes )
            && Seconds.Equals( other.Seconds )
            && Milliseconds.Equals( other.Milliseconds )
            && Microseconds.Equals( other.Microseconds )
            && Ticks.Equals( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    public Period Add(Period other)
    {
        return new Period(
            Years + other.Years,
            Months + other.Months,
            Weeks + other.Weeks,
            Days + other.Days,
            Hours + other.Hours,
            Minutes + other.Minutes,
            Seconds + other.Seconds,
            Milliseconds + other.Milliseconds,
            Microseconds + other.Microseconds,
            Ticks + other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="ticks"/>.
    /// </summary>
    /// <param name="ticks">Ticks to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddTicks(long ticks)
    {
        return SetTicks( Ticks + ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddMicroseconds(long microseconds)
    {
        return SetMicroseconds( Microseconds + microseconds );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddMilliseconds(long milliseconds)
    {
        return SetMilliseconds( Milliseconds + milliseconds );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddSeconds(long seconds)
    {
        return SetSeconds( Seconds + seconds );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddMinutes(long minutes)
    {
        return SetMinutes( Minutes + minutes );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddHours(int hours)
    {
        return SetHours( Hours + hours );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="days"/>.
    /// </summary>
    /// <param name="days">Days to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddDays(int days)
    {
        return SetDays( Days + days );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="weeks"/>.
    /// </summary>
    /// <param name="weeks">Weeks to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddWeeks(int weeks)
    {
        return SetWeeks( Weeks + weeks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="months"/>.
    /// </summary>
    /// <param name="months">Months to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddMonths(int months)
    {
        return SetMonths( Months + months );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding the specified number of <paramref name="years"/>.
    /// </summary>
    /// <param name="years">Years to add.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period AddYears(int years)
    {
        return SetYears( Years + years );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    public Period Subtract(Period other)
    {
        return new Period(
            Years - other.Years,
            Months - other.Months,
            Weeks - other.Weeks,
            Days - other.Days,
            Hours - other.Hours,
            Minutes - other.Minutes,
            Seconds - other.Seconds,
            Milliseconds - other.Milliseconds,
            Microseconds - other.Microseconds,
            Ticks - other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="ticks"/>.
    /// </summary>
    /// <param name="ticks">Ticks to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractTicks(long ticks)
    {
        return SetTicks( Ticks - ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractMicroseconds(long microseconds)
    {
        return SetMicroseconds( Microseconds - microseconds );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractMilliseconds(long milliseconds)
    {
        return SetMilliseconds( Milliseconds - milliseconds );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractSeconds(long seconds)
    {
        return SetSeconds( Seconds - seconds );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractMinutes(long minutes)
    {
        return SetMinutes( Minutes - minutes );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractHours(int hours)
    {
        return SetHours( Hours - hours );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="days"/>.
    /// </summary>
    /// <param name="days">Days to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractDays(int days)
    {
        return SetDays( Days - days );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="weeks"/>.
    /// </summary>
    /// <param name="weeks">Weeks to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractWeeks(int weeks)
    {
        return SetWeeks( Weeks - weeks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="months"/>.
    /// </summary>
    /// <param name="months">Months to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractMonths(int months)
    {
        return SetMonths( Months - months );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting the specified number of <paramref name="years"/>.
    /// </summary>
    /// <param name="years">Years to subtract.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SubtractYears(int years)
    {
        return SetYears( Years - years );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by copying chosen components from the <paramref name="other"/> instance.
    /// </summary>
    /// <param name="other">Other instance to copy components from.</param>
    /// <param name="units"><see cref="PeriodUnits"/> to copy.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    public Period Set(Period other, PeriodUnits units)
    {
        return new Period(
            (units & PeriodUnits.Years) != 0 ? other.Years : Years,
            (units & PeriodUnits.Months) != 0 ? other.Months : Months,
            (units & PeriodUnits.Weeks) != 0 ? other.Weeks : Weeks,
            (units & PeriodUnits.Days) != 0 ? other.Days : Days,
            (units & PeriodUnits.Hours) != 0 ? other.Hours : Hours,
            (units & PeriodUnits.Minutes) != 0 ? other.Minutes : Minutes,
            (units & PeriodUnits.Seconds) != 0 ? other.Seconds : Seconds,
            (units & PeriodUnits.Milliseconds) != 0 ? other.Milliseconds : Milliseconds,
            (units & PeriodUnits.Microseconds) != 0 ? other.Microseconds : Microseconds,
            (units & PeriodUnits.Ticks) != 0 ? other.Ticks : Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting date components.
    /// </summary>
    /// <param name="years">Number of years.</param>
    /// <param name="months">Number of months.</param>
    /// <param name="weeks">Number of weeks.</param>
    /// <param name="days">Number of days.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetDate(int years, int months, int weeks, int days)
    {
        return new Period(
            years,
            months,
            weeks,
            days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting time components.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <param name="ticks">Number of ticks.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetTime(int hours, long minutes, long seconds, long milliseconds, long microseconds, long ticks)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            hours,
            minutes,
            seconds,
            milliseconds,
            microseconds,
            ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of ticks.
    /// </summary>
    /// <param name="ticks">Number of ticks.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetTicks(long ticks)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of microseconds.
    /// </summary>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetMicroseconds(long microseconds)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of milliseconds.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetMilliseconds(long milliseconds)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            Hours,
            Minutes,
            Seconds,
            milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of seconds.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetSeconds(long seconds)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            Hours,
            Minutes,
            seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of minutes.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetMinutes(long minutes)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            Hours,
            minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of hours.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetHours(int hours)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            Days,
            hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of days.
    /// </summary>
    /// <param name="days">Number of days.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetDays(int days)
    {
        return new Period(
            Years,
            Months,
            Weeks,
            days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of weeks.
    /// </summary>
    /// <param name="weeks">Number of weeks.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetWeeks(int weeks)
    {
        return new Period(
            Years,
            Months,
            weeks,
            Days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of months.
    /// </summary>
    /// <param name="months">Number of months.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetMonths(int months)
    {
        return new Period(
            Years,
            months,
            Weeks,
            Days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting the number of years.
    /// </summary>
    /// <param name="years">Number of years.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period SetYears(int years)
    {
        return new Period(
            years,
            Months,
            Weeks,
            Days,
            Hours,
            Minutes,
            Seconds,
            Milliseconds,
            Microseconds,
            Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by negating all components of this instance.
    /// </summary>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Period Negate()
    {
        return new Period(
            -Years,
            -Months,
            -Weeks,
            -Days,
            -Hours,
            -Minutes,
            -Seconds,
            -Milliseconds,
            -Microseconds,
            -Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by calculating an absolute value for all components of this instance.
    /// </summary>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    public Period Abs()
    {
        return new Period(
            Math.Abs( Years ),
            Math.Abs( Months ),
            Math.Abs( Weeks ),
            Math.Abs( Days ),
            Math.Abs( Hours ),
            Math.Abs( Minutes ),
            Math.Abs( Seconds ),
            Math.Abs( Milliseconds ),
            Math.Abs( Microseconds ),
            Math.Abs( Ticks ) );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by setting chosen components to <b>0</b>.
    /// </summary>
    /// <param name="units"><see cref="PeriodUnits"/> to set to <b>0</b>.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    public Period Skip(PeriodUnits units)
    {
        return new Period(
            (units & PeriodUnits.Years) != 0 ? 0 : Years,
            (units & PeriodUnits.Months) != 0 ? 0 : Months,
            (units & PeriodUnits.Weeks) != 0 ? 0 : Weeks,
            (units & PeriodUnits.Days) != 0 ? 0 : Days,
            (units & PeriodUnits.Hours) != 0 ? 0 : Hours,
            (units & PeriodUnits.Minutes) != 0 ? 0 : Minutes,
            (units & PeriodUnits.Seconds) != 0 ? 0 : Seconds,
            (units & PeriodUnits.Milliseconds) != 0 ? 0 : Milliseconds,
            (units & PeriodUnits.Microseconds) != 0 ? 0 : Microseconds,
            (units & PeriodUnits.Ticks) != 0 ? 0 : Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by only copying the chosen components.
    /// </summary>
    /// <param name="units"><see cref="PeriodUnits"/> to copy. Other components will be ignored and set to <b>0</b>.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    public Period Take(PeriodUnits units)
    {
        return new Period(
            (units & PeriodUnits.Years) != 0 ? Years : 0,
            (units & PeriodUnits.Months) != 0 ? Months : 0,
            (units & PeriodUnits.Weeks) != 0 ? Weeks : 0,
            (units & PeriodUnits.Days) != 0 ? Days : 0,
            (units & PeriodUnits.Hours) != 0 ? Hours : 0,
            (units & PeriodUnits.Minutes) != 0 ? Minutes : 0,
            (units & PeriodUnits.Seconds) != 0 ? Seconds : 0,
            (units & PeriodUnits.Milliseconds) != 0 ? Milliseconds : 0,
            (units & PeriodUnits.Microseconds) != 0 ? Microseconds : 0,
            (units & PeriodUnits.Ticks) != 0 ? Ticks : 0 );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by negating the provided <paramref name="a"/>.
    /// </summary>
    /// <param name="a">Operand.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period operator -(Period a)
    {
        return a.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period operator +(Period a, Period b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="Period"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Period"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Period operator -(Period a, Period b)
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
    public static bool operator ==(Period a, Period b)
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
    public static bool operator !=(Period a, Period b)
    {
        return ! a.Equals( b );
    }
}
