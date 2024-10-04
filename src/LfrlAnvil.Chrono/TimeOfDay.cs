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
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a time of day.
/// </summary>
public readonly struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>, IComparable
{
    /// <summary>
    /// Represents the start of the day, that is midnight, 00:00:00.0000000.
    /// </summary>
    public static readonly TimeOfDay Start = new TimeOfDay( 0 );

    /// <summary>
    /// Represents the middle of the day, that is noon, 12:00:00.0000000.
    /// </summary>
    public static readonly TimeOfDay Mid = new TimeOfDay( (ChronoConstants.HoursPerStandardDay >> 1) * ChronoConstants.TicksPerHour );

    /// <summary>
    /// Represents the end of the day, 23:59:59.9999999.
    /// </summary>
    public static readonly TimeOfDay End = new TimeOfDay( ChronoConstants.TicksPerStandardDay - 1 );

    private readonly long _value;

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance.
    /// </summary>
    /// <param name="hour">Hour component.</param>
    /// <param name="minute">Minute component.</param>
    /// <param name="second">Second component.</param>
    /// <param name="millisecond">Millisecond component.</param>
    /// <param name="microsecond">Microsecond component.</param>
    /// <param name="tick">Tick component.</param>
    /// <exception cref="ArgumentOutOfRangeException">When any component is not valid.</exception>
    public TimeOfDay(int hour, int minute = 0, int second = 0, int millisecond = 0, int microsecond = 0, int tick = 0)
    {
        Ensure.IsInRange( hour, 0, ChronoConstants.HoursPerStandardDay - 1 );
        Ensure.IsInRange( minute, 0, ChronoConstants.MinutesPerHour - 1 );
        Ensure.IsInRange( second, 0, ChronoConstants.SecondsPerMinute - 1 );
        Ensure.IsInRange( millisecond, 0, ChronoConstants.MillisecondsPerSecond - 1 );
        Ensure.IsInRange( microsecond, 0, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        Ensure.IsInRange( tick, 0, ChronoConstants.TicksPerMicrosecond - 1 );

        _value = hour * ChronoConstants.TicksPerHour
            + minute * ChronoConstants.TicksPerMinute
            + second * ChronoConstants.TicksPerSecond
            + millisecond * ChronoConstants.TicksPerMillisecond
            + microsecond * ChronoConstants.TicksPerMicrosecond
            + tick;
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance.
    /// </summary>
    /// <param name="timeSpan">Source <see cref="TimeSpan"/>.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When the provided <paramref name="timeSpan"/> is greater than or equal to 24 hours.
    /// </exception>
    public TimeOfDay(TimeSpan timeSpan)
        : this(
            ( int )timeSpan.TotalHours,
            timeSpan.Minutes,
            timeSpan.Seconds,
            timeSpan.Milliseconds,
            timeSpan.Microseconds,
            ( int )(timeSpan.Ticks % ChronoConstants.TicksPerMicrosecond) ) { }

    private TimeOfDay(long value)
    {
        _value = value;
    }

    /// <summary>
    /// Tick component. One tick is equivalent to 100 nanoseconds.
    /// </summary>
    public int Tick => ( int )(_value % ChronoConstants.TicksPerMicrosecond);

    /// <summary>
    /// Microsecond component.
    /// </summary>
    public int Microsecond => ( int )(_value / ChronoConstants.TicksPerMicrosecond % ChronoConstants.MicrosecondsPerMillisecond);

    /// <summary>
    /// Millisecond component.
    /// </summary>
    public int Millisecond => ( int )(_value / ChronoConstants.TicksPerMillisecond % ChronoConstants.MillisecondsPerSecond);

    /// <summary>
    /// Second component.
    /// </summary>
    public int Second => ( int )(_value / ChronoConstants.TicksPerSecond % ChronoConstants.SecondsPerMinute);

    /// <summary>
    /// Minute component.
    /// </summary>
    public int Minute => ( int )(_value / ChronoConstants.TicksPerMinute % ChronoConstants.MinutesPerHour);

    /// <summary>
    /// Hour component.
    /// </summary>
    public int Hour => ( int )(_value / ChronoConstants.TicksPerHour);

    /// <summary>
    /// Returns a string representation of this <see cref="TimeOfDay"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{Hour:00}h {Minute:00}m {Second:00}.{Millisecond:000}{Microsecond:000}{Tick:0}s";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TimeOfDay td && Equals( td );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(TimeOfDay other)
    {
        return _value.Equals( other._value );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is TimeOfDay td ? CompareTo( td ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(TimeOfDay other)
    {
        return _value.CompareTo( other._value );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by inverting this instance, that is subtracting it from 24 hours.
    /// </summary>
    /// <returns>New <see cref="TimeOfDay"/> instance or <see cref="Start"/> when this instance is equal to <see cref="Start"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay Invert()
    {
        return _value == 0 ? this : new TimeOfDay( ChronoConstants.TicksPerStandardDay - _value );
    }

    /// <summary>
    /// Calculates a difference between this instance and the <paramref name="other"/> instance,
    /// where this instance is treated as the end of the range.
    /// </summary>
    /// <param name="other">Instance to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Subtract(TimeOfDay other)
    {
        return Duration.FromTicks( _value - other._value );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by truncating this instance to microseconds.
    /// </summary>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToMicrosecond()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by truncating this instance to milliseconds.
    /// </summary>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToMillisecond()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by truncating this instance to seconds.
    /// </summary>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToSecond()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by truncating this instance to minutes.
    /// </summary>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToMinute()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by truncating this instance to hours.
    /// </summary>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToHour()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by setting the number of ticks in the microsecond component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetTick(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.TicksPerMicrosecond - 1 );
        return new TimeOfDay( _value + value - Tick );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by setting the number of microseconds in the millisecond component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetMicrosecond(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        return new TimeOfDay( _value + (value - Microsecond) * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by setting the number of milliseconds in the second component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetMillisecond(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MillisecondsPerSecond - 1 );
        return new TimeOfDay( _value + (value - Millisecond) * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by setting the number of seconds in the minute component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetSecond(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.SecondsPerMinute - 1 );
        return new TimeOfDay( _value + (value - Second) * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by setting the number of minutes in the hour component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetMinute(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MinutesPerHour - 1 );
        return new TimeOfDay( _value + (value - Minute) * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="TimeOfDay"/> instance by setting the number of hours.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="TimeOfDay"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetHour(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.HoursPerStandardDay - 1 );
        return new TimeOfDay( _value + (value - Hour) * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Converts the provided time of day to <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="t">Value to convert.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator TimeSpan(TimeOfDay t)
    {
        return TimeSpan.FromTicks( t._value );
    }

    /// <summary>
    /// Converts the provided time of day to <see cref="Duration"/>.
    /// </summary>
    /// <param name="t">Value to convert.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator Duration(TimeOfDay t)
    {
        return Duration.FromTicks( t._value );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator -(TimeOfDay a, TimeOfDay b)
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
    public static bool operator ==(TimeOfDay a, TimeOfDay b)
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
    public static bool operator !=(TimeOfDay a, TimeOfDay b)
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
    public static bool operator >(TimeOfDay a, TimeOfDay b)
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
    public static bool operator <=(TimeOfDay a, TimeOfDay b)
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
    public static bool operator <(TimeOfDay a, TimeOfDay b)
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
    public static bool operator >=(TimeOfDay a, TimeOfDay b)
    {
        return a.CompareTo( b ) >= 0;
    }
}
