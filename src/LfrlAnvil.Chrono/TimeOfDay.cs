﻿using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Chrono;

public readonly struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>, IComparable
{
    public static readonly TimeOfDay Start = new TimeOfDay( 0 );
    public static readonly TimeOfDay Mid = new TimeOfDay( (ChronoConstants.HoursPerStandardDay >> 1) * ChronoConstants.TicksPerHour );
    public static readonly TimeOfDay End = new TimeOfDay( ChronoConstants.TicksPerStandardDay - 1 );

    private readonly long _value;

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

    public int Tick => ( int )(_value % ChronoConstants.TicksPerMicrosecond);
    public int Microsecond => ( int )(_value / ChronoConstants.TicksPerMicrosecond % ChronoConstants.MicrosecondsPerMillisecond);
    public int Millisecond => ( int )(_value / ChronoConstants.TicksPerMillisecond % ChronoConstants.MillisecondsPerSecond);
    public int Second => ( int )(_value / ChronoConstants.TicksPerSecond % ChronoConstants.SecondsPerMinute);
    public int Minute => ( int )(_value / ChronoConstants.TicksPerMinute % ChronoConstants.MinutesPerHour);
    public int Hour => ( int )(_value / ChronoConstants.TicksPerHour);

    [Pure]
    public override string ToString()
    {
        return $"{Hour:00}h {Minute:00}m {Second:00}.{Millisecond:000}{Microsecond:000}{Tick:0}s";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return _value.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is TimeOfDay td && Equals( td );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(TimeOfDay other)
    {
        return _value.Equals( other._value );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is TimeOfDay td ? CompareTo( td ) : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(TimeOfDay other)
    {
        return _value.CompareTo( other._value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay Invert()
    {
        return _value == 0 ? this : new TimeOfDay( ChronoConstants.TicksPerStandardDay - _value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Subtract(TimeOfDay other)
    {
        return Duration.FromTicks( _value - other._value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToMicrosecond()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToMillisecond()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToSecond()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToMinute()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay TrimToHour()
    {
        return new TimeOfDay( _value - _value % ChronoConstants.TicksPerHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetTick(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.TicksPerMicrosecond - 1 );
        return new TimeOfDay( _value + value - Tick );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetMicrosecond(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        return new TimeOfDay( _value + (value - Microsecond) * ChronoConstants.TicksPerMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetMillisecond(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MillisecondsPerSecond - 1 );
        return new TimeOfDay( _value + (value - Millisecond) * ChronoConstants.TicksPerMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetSecond(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.SecondsPerMinute - 1 );
        return new TimeOfDay( _value + (value - Second) * ChronoConstants.TicksPerSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetMinute(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MinutesPerHour - 1 );
        return new TimeOfDay( _value + (value - Minute) * ChronoConstants.TicksPerMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TimeOfDay SetHour(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.HoursPerStandardDay - 1 );
        return new TimeOfDay( _value + (value - Hour) * ChronoConstants.TicksPerHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator TimeSpan(TimeOfDay t)
    {
        return TimeSpan.FromTicks( t._value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator Duration(TimeOfDay t)
    {
        return Duration.FromTicks( t._value );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator -(TimeOfDay a, TimeOfDay b)
    {
        return a.Subtract( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(TimeOfDay a, TimeOfDay b)
    {
        return a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(TimeOfDay a, TimeOfDay b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(TimeOfDay a, TimeOfDay b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(TimeOfDay a, TimeOfDay b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(TimeOfDay a, TimeOfDay b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(TimeOfDay a, TimeOfDay b)
    {
        return a.CompareTo( b ) >= 0;
    }
}
