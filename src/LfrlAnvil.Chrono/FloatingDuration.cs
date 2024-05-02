using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a duration in time, or elapsed time, or a difference between two timestamps, with sub-tick precision.
/// </summary>
public readonly struct FloatingDuration : IEquatable<FloatingDuration>, IComparable<FloatingDuration>, IComparable
{
    /// <summary>
    /// Specifies the <see cref="FloatingDuration"/> of <b>0</b> length.
    /// </summary>
    public static readonly FloatingDuration Zero = new FloatingDuration( 0m );

    /// <summary>
    /// Specifies maximum possible <see cref="FloatingDuration"/>.
    /// </summary>
    public static readonly FloatingDuration MinValue = new FloatingDuration( decimal.MinValue );

    /// <summary>
    /// Specifies minimum possible <see cref="FloatingDuration"/>.
    /// </summary>
    public static readonly FloatingDuration MaxValue = new FloatingDuration( decimal.MaxValue );

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="ticks">Number of ticks.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(decimal ticks)
    {
        Ticks = ticks;
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <param name="ticks">Number of ticks.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, decimal ticks = 0)
        : this(
            hours * ChronoConstants.TicksPerHour
            + minutes * ChronoConstants.TicksPerMinute
            + seconds * ChronoConstants.TicksPerSecond
            + milliseconds * ChronoConstants.TicksPerMillisecond
            + microseconds * ChronoConstants.TicksPerMicrosecond
            + ticks ) { }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="timeSpan">Source <see cref="TimeSpan"/>.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(TimeSpan timeSpan)
        : this( timeSpan.Ticks ) { }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="duration">Source <see cref="Duration"/>.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration(Duration duration)
        : this( duration.Ticks ) { }

    /// <summary>
    /// Total number of ticks. One tick is equivalent to 100 nanoseconds.
    /// </summary>
    public decimal Ticks { get; }

    /// <summary>
    /// Total number of full ticks.
    /// </summary>
    public long FullTicks => ( long )Ticks;

    /// <summary>
    /// Total number of full microseconds.
    /// </summary>
    public long FullMicroseconds => ( long )(Ticks / ChronoConstants.TicksPerMicrosecond);

    /// <summary>
    /// Total number of full milliseconds.
    /// </summary>
    public long FullMilliseconds => ( long )(Ticks / ChronoConstants.TicksPerMillisecond);

    /// <summary>
    /// Total number of full seconds.
    /// </summary>
    public long FullSeconds => ( long )(Ticks / ChronoConstants.TicksPerSecond);

    /// <summary>
    /// Total number of full minutes.
    /// </summary>
    public long FullMinutes => ( long )(Ticks / ChronoConstants.TicksPerMinute);

    /// <summary>
    /// Total number of full hours.
    /// </summary>
    public long FullHours => ( long )(Ticks / ChronoConstants.TicksPerHour);

    /// <summary>
    /// Number of ticks in the microsecond component.
    /// </summary>
    public decimal TicksInMicrosecond => Ticks % ChronoConstants.TicksPerMicrosecond;

    /// <summary>
    /// Number of microseconds in the millisecond component.
    /// </summary>
    public int MicrosecondsInMillisecond => ( int )(FullMicroseconds % ChronoConstants.MicrosecondsPerMillisecond);

    /// <summary>
    /// Number of milliseconds in the second component.
    /// </summary>
    public int MillisecondsInSecond => ( int )(FullMilliseconds % ChronoConstants.MillisecondsPerSecond);

    /// <summary>
    /// Number of seconds in the minute component.
    /// </summary>
    public int SecondsInMinute => ( int )(FullSeconds % ChronoConstants.SecondsPerMinute);

    /// <summary>
    /// Number of minutes in the hour component.
    /// </summary>
    public int MinutesInHour => ( int )(FullMinutes % ChronoConstants.MinutesPerHour);

    /// <summary>
    /// Total number of microseconds.
    /// </summary>
    public decimal TotalMicroseconds => Ticks / ChronoConstants.TicksPerMicrosecond;

    /// <summary>
    /// Total number of milliseconds.
    /// </summary>
    public decimal TotalMilliseconds => Ticks / ChronoConstants.TicksPerMillisecond;

    /// <summary>
    /// Total number of seconds.
    /// </summary>
    public decimal TotalSeconds => Ticks / ChronoConstants.TicksPerSecond;

    /// <summary>
    /// Total number of minutes.
    /// </summary>
    public decimal TotalMinutes => Ticks / ChronoConstants.TicksPerMinute;

    /// <summary>
    /// Total number of hours.
    /// </summary>
    public decimal TotalHours => Ticks / ChronoConstants.TicksPerHour;

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="ticks">Number of ticks.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromTicks(decimal ticks)
    {
        return new FloatingDuration( ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromMicroseconds(decimal microseconds)
    {
        return new FloatingDuration( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromMilliseconds(decimal milliseconds)
    {
        return new FloatingDuration( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromSeconds(decimal seconds)
    {
        return new FloatingDuration( seconds * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromMinutes(decimal minutes)
    {
        return new FloatingDuration( minutes * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration FromHours(decimal hours)
    {
        return new FloatingDuration( hours * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="FloatingDuration"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{TotalSeconds.ToString( CultureInfo.InvariantCulture )} second(s)";
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public override int GetHashCode()
    {
        return Ticks.GetHashCode();
    }

    /// <inheritdoc />
    [Pure]
    public override bool Equals(object? obj)
    {
        return obj is FloatingDuration d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(FloatingDuration other)
    {
        return Ticks.Equals( other.Ticks );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is FloatingDuration d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(FloatingDuration other)
    {
        return Ticks.CompareTo( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by negating this instance.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Negate()
    {
        return new FloatingDuration( -Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Abs()
    {
        return new FloatingDuration( Math.Abs( Ticks ) );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Add(FloatingDuration other)
    {
        return AddTicks( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding the specified number of <paramref name="ticks"/>.
    /// </summary>
    /// <param name="ticks">Ticks to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddTicks(decimal ticks)
    {
        return new FloatingDuration( Ticks + ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddMicroseconds(decimal microseconds)
    {
        return AddTicks( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddMilliseconds(decimal milliseconds)
    {
        return AddTicks( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddSeconds(decimal seconds)
    {
        return AddTicks( seconds * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddMinutes(decimal minutes)
    {
        return AddTicks( minutes * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to add.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration AddHours(decimal hours)
    {
        return AddTicks( hours * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Subtract(FloatingDuration other)
    {
        return SubtractTicks( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting the specified number of <paramref name="ticks"/>.
    /// </summary>
    /// <param name="ticks">Ticks to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractTicks(decimal ticks)
    {
        return AddTicks( -ticks );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractMicroseconds(decimal microseconds)
    {
        return SubtractTicks( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractMilliseconds(decimal milliseconds)
    {
        return SubtractTicks( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractSeconds(decimal seconds)
    {
        return SubtractTicks( seconds * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractMinutes(decimal minutes)
    {
        return SubtractTicks( minutes * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to subtract.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration SubtractHours(decimal hours)
    {
        return SubtractTicks( hours * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by multiplying this instance by the provided <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">Percent to multiply by.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Multiply(Percent percent)
    {
        return new FloatingDuration( Ticks * percent );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by multiplying this instance by the provided <paramref name="multiplier"/>.
    /// </summary>
    /// <param name="multiplier">Value to multiply by.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Multiply(decimal multiplier)
    {
        return FromTicks( Ticks * multiplier );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by dividing this instance by the provided <paramref name="divisor"/>.
    /// </summary>
    /// <param name="divisor">Value to divide by.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="divisor"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration Divide(decimal divisor)
    {
        return FromTicks( Ticks / divisor );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by truncating this instance to ticks.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToTick()
    {
        return FromTicks( Math.Truncate( Ticks ) );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by truncating this instance to microseconds.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToMicrosecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by truncating this instance to milliseconds.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToMillisecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by truncating this instance to seconds.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToSecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by truncating this instance to minutes.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToMinute()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by truncating this instance to hours.
    /// </summary>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public FloatingDuration TrimToHour()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by setting the number of ticks in the microsecond component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
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

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by setting the number of microseconds in the millisecond component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
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

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by setting the number of milliseconds in the second component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
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

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by setting the number of seconds in the minute component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
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

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by setting the number of minutes in the hour component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
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

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by setting the number of hours.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
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

    /// <summary>
    /// Coverts the provided floating duration to <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="d">Value to convert.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator TimeSpan(FloatingDuration d)
    {
        return TimeSpan.FromTicks( ( long )Math.Round( d.Ticks, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Coverts the provided floating duration to <see cref="Duration"/>.
    /// </summary>
    /// <param name="d">Value to convert.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static explicit operator Duration(FloatingDuration d)
    {
        return Duration.FromTicks( ( long )Math.Round( d.Ticks, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by negating the provided <paramref name="a"/>.
    /// </summary>
    /// <param name="a">Operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator -(FloatingDuration a)
    {
        return a.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator +(FloatingDuration a, FloatingDuration b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator -(FloatingDuration a, FloatingDuration b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator *(FloatingDuration a, decimal b)
    {
        return a.Multiply( b );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by dividing <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static FloatingDuration operator /(FloatingDuration a, decimal b)
    {
        return a.Divide( b );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    public static FloatingDuration operator *(FloatingDuration a, Percent b)
    {
        return a.Multiply( b );
    }

    /// <summary>
    /// Creates a new <see cref="FloatingDuration"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    public static FloatingDuration operator *(Percent a, FloatingDuration b)
    {
        return b.Multiply( a );
    }

    /// <summary>
    /// Checks if <paramref name="a"/> is equal to <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns><b>true</b> when operands are equal, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool operator ==(FloatingDuration a, FloatingDuration b)
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
    public static bool operator !=(FloatingDuration a, FloatingDuration b)
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
    public static bool operator >(FloatingDuration a, FloatingDuration b)
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
    public static bool operator <=(FloatingDuration a, FloatingDuration b)
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
    public static bool operator <(FloatingDuration a, FloatingDuration b)
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
