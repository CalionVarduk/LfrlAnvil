using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Chrono;

public readonly struct FloatingDuration : IEquatable<FloatingDuration>, IComparable<FloatingDuration>, IComparable
{
    public static readonly FloatingDuration Zero = new FloatingDuration( 0m );
    public static readonly FloatingDuration MinValue = new FloatingDuration( decimal.MinValue );
    public static readonly FloatingDuration MaxValue = new FloatingDuration( decimal.MaxValue );

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(decimal ticks)
    {
        Ticks = ticks;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, decimal ticks = 0)
        : this(
            hours * ChronoConstants.TicksPerHour +
            minutes * ChronoConstants.TicksPerMinute +
            seconds * ChronoConstants.TicksPerSecond +
            milliseconds * ChronoConstants.TicksPerMillisecond +
            microseconds * ChronoConstants.TicksPerMicrosecond +
            ticks ) { }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(TimeSpan timeSpan)
        : this( timeSpan.Ticks ) { }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(Duration duration)
        : this( duration.Ticks ) { }

    public decimal Ticks { get; }
    public long FullTicks => (long)Ticks;
    public long FullMicroseconds => (long)(Ticks / ChronoConstants.TicksPerMicrosecond);
    public long FullMilliseconds => (long)(Ticks / ChronoConstants.TicksPerMillisecond);
    public long FullSeconds => (long)(Ticks / ChronoConstants.TicksPerSecond);
    public long FullMinutes => (long)(Ticks / ChronoConstants.TicksPerMinute);
    public long FullHours => (long)(Ticks / ChronoConstants.TicksPerHour);
    public decimal TicksInMicrosecond => Ticks % ChronoConstants.TicksPerMicrosecond;
    public int MicrosecondsInMillisecond => (int)(FullMicroseconds % ChronoConstants.MicrosecondsPerMillisecond);
    public int MillisecondsInSecond => (int)(FullMilliseconds % ChronoConstants.MillisecondsPerSecond);
    public int SecondsInMinute => (int)(FullSeconds % ChronoConstants.SecondsPerMinute);
    public int MinutesInHour => (int)(FullMinutes % ChronoConstants.MinutesPerHour);
    public decimal TotalMicroseconds => Ticks / ChronoConstants.TicksPerMicrosecond;
    public decimal TotalMilliseconds => Ticks / ChronoConstants.TicksPerMillisecond;
    public decimal TotalSeconds => Ticks / ChronoConstants.TicksPerSecond;
    public decimal TotalMinutes => Ticks / ChronoConstants.TicksPerMinute;
    public decimal TotalHours => Ticks / ChronoConstants.TicksPerHour;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromTicks(decimal ticks)
    {
        return new FloatingDuration( ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromMicroseconds(decimal microseconds)
    {
        return new FloatingDuration( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromMilliseconds(decimal milliseconds)
    {
        return new FloatingDuration( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromSeconds(decimal seconds)
    {
        return new FloatingDuration( seconds * ChronoConstants.TicksPerSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromMinutes(decimal minutes)
    {
        return new FloatingDuration( minutes * ChronoConstants.TicksPerMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromHours(decimal hours)
    {
        return new FloatingDuration( hours * ChronoConstants.TicksPerHour );
    }

    [Pure]
    public override string ToString()
    {
        return $"{TotalSeconds.ToString( CultureInfo.InvariantCulture )} second(s)";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Ticks.GetHashCode();
    }

    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is FloatingDuration d && Equals( d );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(FloatingDuration other)
    {
        return Ticks.Equals( other.Ticks );
    }

    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is FloatingDuration d ? CompareTo( d ) : 1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(FloatingDuration other)
    {
        return Ticks.CompareTo( other.Ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Negate()
    {
        return new FloatingDuration( -Ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Abs()
    {
        return new FloatingDuration( Math.Abs( Ticks ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Add(FloatingDuration other)
    {
        return AddTicks( other.Ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddTicks(decimal ticks)
    {
        return new FloatingDuration( Ticks + ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddMicroseconds(decimal microseconds)
    {
        return AddTicks( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddMilliseconds(decimal milliseconds)
    {
        return AddTicks( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddSeconds(decimal seconds)
    {
        return AddTicks( seconds * ChronoConstants.TicksPerSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddMinutes(decimal minutes)
    {
        return AddTicks( minutes * ChronoConstants.TicksPerMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddHours(decimal hours)
    {
        return AddTicks( hours * ChronoConstants.TicksPerHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Subtract(FloatingDuration other)
    {
        return SubtractTicks( other.Ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractTicks(decimal ticks)
    {
        return AddTicks( -ticks );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractMicroseconds(decimal microseconds)
    {
        return SubtractTicks( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractMilliseconds(decimal milliseconds)
    {
        return SubtractTicks( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractSeconds(decimal seconds)
    {
        return SubtractTicks( seconds * ChronoConstants.TicksPerSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractMinutes(decimal minutes)
    {
        return SubtractTicks( minutes * ChronoConstants.TicksPerMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractHours(decimal hours)
    {
        return SubtractTicks( hours * ChronoConstants.TicksPerHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Multiply(Percent percent)
    {
        return new FloatingDuration( Ticks * percent );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Multiply(decimal multiplier)
    {
        return FromTicks( Ticks * multiplier );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Divide(decimal divisor)
    {
        return FromTicks( Ticks / divisor );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToTick()
    {
        return FromTicks( Math.Truncate( Ticks ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToMicrosecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToMillisecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToSecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToMinute()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToHour()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SetTicksInMicrosecond(decimal value)
    {
        return Ticks switch
        {
            > 0 => SetTicksInMicrosecondForPositive( value ),
            < 0 => SetTicksInMicrosecondForNegative( value ),
            _ => SetTicksInMicrosecondForZero( value )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SetMicrosecondsInMillisecond(int value)
    {
        return Ticks switch
        {
            > 0 => SetMicrosecondsInMillisecondForPositive( value ),
            < 0 => SetMicrosecondsInMillisecondForNegative( value ),
            _ => SetMicrosecondsInMillisecondForZero( value )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SetMillisecondsInSecond(int value)
    {
        return Ticks switch
        {
            > 0 => SetMillisecondsInSecondForPositive( value ),
            < 0 => SetMillisecondsInSecondForNegative( value ),
            _ => SetMillisecondsInSecondForZero( value )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SetSecondsInMinute(int value)
    {
        return Ticks switch
        {
            > 0 => SetSecondsInMinuteForPositive( value ),
            < 0 => SetSecondsInMinuteForNegative( value ),
            _ => SetSecondsInMinuteForZero( value )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SetMinutesInHour(int value)
    {
        return Ticks switch
        {
            > 0 => SetMinutesInHourForPositive( value ),
            < 0 => SetMinutesInHourForNegative( value ),
            _ => SetMinutesInHourForZero( value )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SetHours(long value)
    {
        return Ticks switch
        {
            > 0 => SetHoursForPositive( value ),
            < 0 => SetHoursForNegative( value ),
            _ => SetHoursForZero( value )
        };
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator TimeSpan(FloatingDuration d)
    {
        return TimeSpan.FromTicks( (long)Math.Round( d.Ticks, MidpointRounding.AwayFromZero ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator Duration(FloatingDuration d)
    {
        return Duration.FromTicks( (long)Math.Round( d.Ticks, MidpointRounding.AwayFromZero ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator -(FloatingDuration a)
    {
        return a.Negate();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator +(FloatingDuration a, FloatingDuration b)
    {
        return a.Add( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator -(FloatingDuration a, FloatingDuration b)
    {
        return a.Subtract( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator *(FloatingDuration a, decimal b)
    {
        return a.Multiply( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator /(FloatingDuration a, decimal b)
    {
        return a.Divide( b );
    }

    [Pure]
    public static FloatingDuration operator *(FloatingDuration left, Percent right)
    {
        return left.Multiply( right );
    }

    [Pure]
    public static FloatingDuration operator *(Percent left, FloatingDuration right)
    {
        return right.Multiply( left );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(FloatingDuration a, FloatingDuration b)
    {
        return a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator !=(FloatingDuration a, FloatingDuration b)
    {
        return ! a.Equals( b );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >(FloatingDuration a, FloatingDuration b)
    {
        return a.CompareTo( b ) > 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <=(FloatingDuration a, FloatingDuration b)
    {
        return a.CompareTo( b ) <= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator <(FloatingDuration a, FloatingDuration b)
    {
        return a.CompareTo( b ) < 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator >=(FloatingDuration a, FloatingDuration b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetTicksInMicrosecondForPositive(decimal value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.TicksPerMicrosecond - 1 );
        return AddTicks( value - TicksInMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetTicksInMicrosecondForNegative(decimal value)
    {
        Ensure.IsInRange( value, -ChronoConstants.TicksPerMicrosecond + 1, 0 );
        return AddTicks( value - TicksInMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetTicksInMicrosecondForZero(decimal value)
    {
        Ensure.IsInRange( value, -ChronoConstants.TicksPerMicrosecond + 1, ChronoConstants.TicksPerMicrosecond - 1 );
        return AddTicks( value - TicksInMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMicrosecondsInMillisecondForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        return AddMicroseconds( value - MicrosecondsInMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMicrosecondsInMillisecondForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MicrosecondsPerMillisecond + 1, 0 );
        return AddMicroseconds( value - MicrosecondsInMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMicrosecondsInMillisecondForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MicrosecondsPerMillisecond + 1, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        return AddMicroseconds( value - MicrosecondsInMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMillisecondsInSecondForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MillisecondsPerSecond - 1 );
        return AddMilliseconds( value - MillisecondsInSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMillisecondsInSecondForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MillisecondsPerSecond + 1, 0 );
        return AddMilliseconds( value - MillisecondsInSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMillisecondsInSecondForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MillisecondsPerSecond + 1, ChronoConstants.MillisecondsPerSecond - 1 );
        return AddMilliseconds( value - MillisecondsInSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetSecondsInMinuteForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.SecondsPerMinute - 1 );
        return AddSeconds( value - SecondsInMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetSecondsInMinuteForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.SecondsPerMinute + 1, 0 );
        return AddSeconds( value - SecondsInMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetSecondsInMinuteForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.SecondsPerMinute + 1, ChronoConstants.SecondsPerMinute - 1 );
        return AddSeconds( value - SecondsInMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMinutesInHourForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MinutesPerHour - 1 );
        return AddMinutes( value - MinutesInHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMinutesInHourForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MinutesPerHour + 1, 0 );
        return AddMinutes( value - MinutesInHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetMinutesInHourForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MinutesPerHour + 1, ChronoConstants.MinutesPerHour - 1 );
        return AddMinutes( value - MinutesInHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetHoursForPositive(long value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0 );
        return AddHours( value - FullHours );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetHoursForNegative(long value)
    {
        Ensure.IsLessThanOrEqualTo( value, 0 );
        return AddHours( value - FullHours );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private FloatingDuration SetHoursForZero(long value)
    {
        return AddHours( value - FullHours );
    }
}
