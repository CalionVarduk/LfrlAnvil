using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Numerics;

namespace LfrlAnvil.Chrono;

/// <summary>
/// Represents a duration in time, or elapsed time, or a difference between two timestamps.
/// </summary>
public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>, IComparable
{
    /// <summary>
    /// Specifies the <see cref="Duration"/> of <b>0</b> length.
    /// </summary>
    public static readonly Duration Zero = new Duration( 0 );

    /// <summary>
    /// Specifies maximum possible <see cref="Duration"/>.
    /// </summary>
    public static readonly Duration MinValue = new Duration( long.MinValue );

    /// <summary>
    /// Specifies minimum possible <see cref="Duration"/>.
    /// </summary>
    public static readonly Duration MaxValue = new Duration( long.MaxValue );

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="ticks">Number of ticks.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration(long ticks)
    {
        Ticks = ticks;
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <param name="minutes">Number of minutes.</param>
    /// <param name="seconds">Number of seconds.</param>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <param name="ticks">Number of ticks.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration(int hours, int minutes, int seconds = 0, int milliseconds = 0, int microseconds = 0, int ticks = 0)
        : this(
            hours * ChronoConstants.TicksPerHour
            + minutes * ChronoConstants.TicksPerMinute
            + seconds * ChronoConstants.TicksPerSecond
            + milliseconds * ChronoConstants.TicksPerMillisecond
            + microseconds * ChronoConstants.TicksPerMicrosecond
            + ticks ) { }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="timeSpan">Source <see cref="TimeSpan"/>.</param>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration(TimeSpan timeSpan)
        : this( timeSpan.Ticks ) { }

    /// <summary>
    /// Total number of ticks. One tick is equivalent to 100 nanoseconds.
    /// </summary>
    public long Ticks { get; }

    /// <summary>
    /// Total number of full microseconds.
    /// </summary>
    public long FullMicroseconds => Ticks / ChronoConstants.TicksPerMicrosecond;

    /// <summary>
    /// Total number of full milliseconds.
    /// </summary>
    public long FullMilliseconds => Ticks / ChronoConstants.TicksPerMillisecond;

    /// <summary>
    /// Total number of full seconds.
    /// </summary>
    public long FullSeconds => Ticks / ChronoConstants.TicksPerSecond;

    /// <summary>
    /// Total number of full minutes.
    /// </summary>
    public long FullMinutes => Ticks / ChronoConstants.TicksPerMinute;

    /// <summary>
    /// Total number of full hours.
    /// </summary>
    public long FullHours => Ticks / ChronoConstants.TicksPerHour;

    /// <summary>
    /// Number of ticks in the microsecond component.
    /// </summary>
    public int TicksInMicrosecond => ( int )(Ticks % ChronoConstants.TicksPerMicrosecond);

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
    public double TotalMicroseconds => ( double )Ticks / ChronoConstants.TicksPerMicrosecond;

    /// <summary>
    /// Total number of milliseconds.
    /// </summary>
    public double TotalMilliseconds => ( double )Ticks / ChronoConstants.TicksPerMillisecond;

    /// <summary>
    /// Total number of seconds.
    /// </summary>
    public double TotalSeconds => ( double )Ticks / ChronoConstants.TicksPerSecond;

    /// <summary>
    /// Total number of minutes.
    /// </summary>
    public double TotalMinutes => ( double )Ticks / ChronoConstants.TicksPerMinute;

    /// <summary>
    /// Total number of hours.
    /// </summary>
    public double TotalHours => ( double )Ticks / ChronoConstants.TicksPerHour;

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="ticks">Number of ticks.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromTicks(long ticks)
    {
        return new Duration( ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromMicroseconds(double microseconds)
    {
        return new Duration( ( long )Math.Round( microseconds * ChronoConstants.TicksPerMicrosecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="microseconds">Number of microseconds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromMicroseconds(long microseconds)
    {
        return new Duration( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromMilliseconds(double milliseconds)
    {
        return new Duration( ( long )Math.Round( milliseconds * ChronoConstants.TicksPerMillisecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="milliseconds">Number of milliseconds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromMilliseconds(long milliseconds)
    {
        return new Duration( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromSeconds(double seconds)
    {
        return new Duration( ( long )Math.Round( seconds * ChronoConstants.TicksPerSecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="seconds">Number of seconds.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromSeconds(long seconds)
    {
        return new Duration( seconds * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromMinutes(double minutes)
    {
        return new Duration( ( long )Math.Round( minutes * ChronoConstants.TicksPerMinute, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="minutes">Number of minutes.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromMinutes(long minutes)
    {
        return new Duration( minutes * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromHours(double hours)
    {
        return new Duration( ( long )Math.Round( hours * ChronoConstants.TicksPerHour, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance.
    /// </summary>
    /// <param name="hours">Number of hours.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration FromHours(long hours)
    {
        return new Duration( hours * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="Duration"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{(( decimal )TotalSeconds).ToString( CultureInfo.InvariantCulture )} second(s)";
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
        return obj is Duration d && Equals( d );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool Equals(Duration other)
    {
        return Ticks.Equals( other.Ticks );
    }

    /// <inheritdoc />
    [Pure]
    public int CompareTo(object? obj)
    {
        return obj is Duration d ? CompareTo( d ) : throw new ArgumentException( ExceptionResources.InvalidType, nameof( obj ) );
    }

    /// <inheritdoc />
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public int CompareTo(Duration other)
    {
        return Ticks.CompareTo( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by negating this instance.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Negate()
    {
        return new Duration( -Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by calculating an absolute value from this instance.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Abs()
    {
        return new Duration( Math.Abs( Ticks ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding <paramref name="other"/> to this instance.
    /// </summary>
    /// <param name="other">Other instance to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Add(Duration other)
    {
        return AddTicks( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="ticks"/>.
    /// </summary>
    /// <param name="ticks">Ticks to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddTicks(long ticks)
    {
        return new Duration( Ticks + ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddMicroseconds(double microseconds)
    {
        return AddTicks( ( long )Math.Round( microseconds * ChronoConstants.TicksPerMicrosecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddMicroseconds(long microseconds)
    {
        return AddTicks( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddMilliseconds(double milliseconds)
    {
        return AddTicks( ( long )Math.Round( milliseconds * ChronoConstants.TicksPerMillisecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddMilliseconds(long milliseconds)
    {
        return AddTicks( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddSeconds(double seconds)
    {
        return AddTicks( ( long )Math.Round( seconds * ChronoConstants.TicksPerSecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddSeconds(long seconds)
    {
        return AddTicks( seconds * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddMinutes(double minutes)
    {
        return AddTicks( ( long )Math.Round( minutes * ChronoConstants.TicksPerMinute, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddMinutes(long minutes)
    {
        return AddTicks( minutes * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddHours(double hours)
    {
        return AddTicks( ( long )Math.Round( hours * ChronoConstants.TicksPerHour, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to add.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration AddHours(long hours)
    {
        return AddTicks( hours * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting <paramref name="other"/> from this instance.
    /// </summary>
    /// <param name="other">Other instance to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Subtract(Duration other)
    {
        return SubtractTicks( other.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="ticks"/>.
    /// </summary>
    /// <param name="ticks">Ticks to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractTicks(long ticks)
    {
        return AddTicks( -ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractMicroseconds(double microseconds)
    {
        return SubtractTicks( ( long )Math.Round( microseconds * ChronoConstants.TicksPerMicrosecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="microseconds"/>.
    /// </summary>
    /// <param name="microseconds">Microseconds to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractMicroseconds(long microseconds)
    {
        return SubtractTicks( microseconds * ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractMilliseconds(double milliseconds)
    {
        return SubtractTicks( ( long )Math.Round( milliseconds * ChronoConstants.TicksPerMillisecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="milliseconds"/>.
    /// </summary>
    /// <param name="milliseconds">Milliseconds to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractMilliseconds(long milliseconds)
    {
        return SubtractTicks( milliseconds * ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractSeconds(double seconds)
    {
        return SubtractTicks( ( long )Math.Round( seconds * ChronoConstants.TicksPerSecond, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="seconds"/>.
    /// </summary>
    /// <param name="seconds">Seconds to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractSeconds(long seconds)
    {
        return SubtractTicks( seconds * ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractMinutes(double minutes)
    {
        return SubtractTicks( ( long )Math.Round( minutes * ChronoConstants.TicksPerMinute, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="minutes"/>.
    /// </summary>
    /// <param name="minutes">Minutes to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractMinutes(long minutes)
    {
        return SubtractTicks( minutes * ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractHours(double hours)
    {
        return SubtractTicks( ( long )Math.Round( hours * ChronoConstants.TicksPerHour, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting the specified number of <paramref name="hours"/>.
    /// </summary>
    /// <param name="hours">Hours to subtract.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SubtractHours(long hours)
    {
        return SubtractTicks( hours * ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by multiplying this instance by the provided <paramref name="percent"/>.
    /// </summary>
    /// <param name="percent">Percent to multiply by.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Multiply(Percent percent)
    {
        return new Duration( Ticks * percent );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by multiplying this instance by the provided <paramref name="multiplier"/>.
    /// </summary>
    /// <param name="multiplier">Value to multiply by.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Multiply(double multiplier)
    {
        return FromTicks( ( long )Math.Round( Ticks * multiplier, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by dividing this instance by the provided <paramref name="divisor"/>.
    /// </summary>
    /// <param name="divisor">Value to divide by.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="divisor"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration Divide(double divisor)
    {
        if ( divisor == 0 )
            ExceptionThrower.Throw( new DivideByZeroException( ExceptionResources.DividedByZero ) );

        return FromTicks( ( long )Math.Round( Ticks / divisor, MidpointRounding.AwayFromZero ) );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by truncating this instance to microseconds.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration TrimToMicrosecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMicrosecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by truncating this instance to milliseconds.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration TrimToMillisecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMillisecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by truncating this instance to seconds.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration TrimToSecond()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerSecond );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by truncating this instance to minutes.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration TrimToMinute()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerMinute );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by truncating this instance to hours.
    /// </summary>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration TrimToHour()
    {
        return SubtractTicks( Ticks % ChronoConstants.TicksPerHour );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by setting the number of ticks in the microsecond component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SetTicksInMicrosecond(int value)
    {
        return Ticks switch
        {
            > 0 => SetTicksInMicrosecondForPositive( value ),
            < 0 => SetTicksInMicrosecondForNegative( value ),
            _ => SetTicksInMicrosecondForZero( value )
        };
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by setting the number of microseconds in the millisecond component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SetMicrosecondsInMillisecond(int value)
    {
        return Ticks switch
        {
            > 0 => SetMicrosecondsInMillisecondForPositive( value ),
            < 0 => SetMicrosecondsInMillisecondForNegative( value ),
            _ => SetMicrosecondsInMillisecondForZero( value )
        };
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by setting the number of milliseconds in the second component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SetMillisecondsInSecond(int value)
    {
        return Ticks switch
        {
            > 0 => SetMillisecondsInSecondForPositive( value ),
            < 0 => SetMillisecondsInSecondForNegative( value ),
            _ => SetMillisecondsInSecondForZero( value )
        };
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by setting the number of seconds in the minute component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SetSecondsInMinute(int value)
    {
        return Ticks switch
        {
            > 0 => SetSecondsInMinuteForPositive( value ),
            < 0 => SetSecondsInMinuteForNegative( value ),
            _ => SetSecondsInMinuteForZero( value )
        };
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by setting the number of minutes in the hour component.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is not in a valid range.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SetMinutesInHour(int value)
    {
        return Ticks switch
        {
            > 0 => SetMinutesInHourForPositive( value ),
            < 0 => SetMinutesInHourForNegative( value ),
            _ => SetMinutesInHourForZero( value )
        };
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by setting the number of hours.
    /// </summary>
    /// <param name="value">New value.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Duration SetHours(long value)
    {
        return Ticks switch
        {
            > 0 => SetHoursForPositive( value ),
            < 0 => SetHoursForNegative( value ),
            _ => SetHoursForZero( value )
        };
    }

    /// <summary>
    /// Coverts the provided duration to <see cref="TimeSpan"/>.
    /// </summary>
    /// <param name="d">Value to convert.</param>
    /// <returns>New <see cref="TimeSpan"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator TimeSpan(Duration d)
    {
        return TimeSpan.FromTicks( d.Ticks );
    }

    /// <summary>
    /// Coverts the provided duration to <see cref="FloatingDuration"/>.
    /// </summary>
    /// <param name="d">Value to convert.</param>
    /// <returns>New <see cref="FloatingDuration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator FloatingDuration(Duration d)
    {
        return FloatingDuration.FromTicks( d.Ticks );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by negating the provided <paramref name="a"/>.
    /// </summary>
    /// <param name="a">Operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator -(Duration a)
    {
        return a.Negate();
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by adding <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator +(Duration a, Duration b)
    {
        return a.Add( b );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by subtracting <paramref name="b"/> from <paramref name="a"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator -(Duration a, Duration b)
    {
        return a.Subtract( b );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator *(Duration a, double b)
    {
        return a.Multiply( b );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by dividing <paramref name="a"/> by <paramref name="b"/>.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    /// <exception cref="DivideByZeroException">When <paramref name="b"/> is equal to <b>0</b>.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Duration operator /(Duration a, double b)
    {
        return a.Divide( b );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    public static Duration operator *(Duration a, Percent b)
    {
        return a.Multiply( b );
    }

    /// <summary>
    /// Creates a new <see cref="Duration"/> instance by multiplying <paramref name="a"/> and <paramref name="b"/> together.
    /// </summary>
    /// <param name="a">First operand.</param>
    /// <param name="b">Second operand.</param>
    /// <returns>New <see cref="Duration"/> instance.</returns>
    [Pure]
    public static Duration operator *(Percent a, Duration b)
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
    public static bool operator ==(Duration a, Duration b)
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
    public static bool operator !=(Duration a, Duration b)
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
    public static bool operator >(Duration a, Duration b)
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
    public static bool operator <=(Duration a, Duration b)
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
    public static bool operator <(Duration a, Duration b)
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
    public static bool operator >=(Duration a, Duration b)
    {
        return a.CompareTo( b ) >= 0;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetTicksInMicrosecondForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.TicksPerMicrosecond - 1 );
        return AddTicks( value - TicksInMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetTicksInMicrosecondForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.TicksPerMicrosecond + 1, 0 );
        return AddTicks( value - TicksInMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetTicksInMicrosecondForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.TicksPerMicrosecond + 1, ChronoConstants.TicksPerMicrosecond - 1 );
        return AddTicks( value - TicksInMicrosecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMicrosecondsInMillisecondForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        return AddMicroseconds( value - MicrosecondsInMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMicrosecondsInMillisecondForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MicrosecondsPerMillisecond + 1, 0 );
        return AddMicroseconds( value - MicrosecondsInMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMicrosecondsInMillisecondForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MicrosecondsPerMillisecond + 1, ChronoConstants.MicrosecondsPerMillisecond - 1 );
        return AddMicroseconds( value - MicrosecondsInMillisecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMillisecondsInSecondForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MillisecondsPerSecond - 1 );
        return AddMilliseconds( value - MillisecondsInSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMillisecondsInSecondForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MillisecondsPerSecond + 1, 0 );
        return AddMilliseconds( value - MillisecondsInSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMillisecondsInSecondForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MillisecondsPerSecond + 1, ChronoConstants.MillisecondsPerSecond - 1 );
        return AddMilliseconds( value - MillisecondsInSecond );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetSecondsInMinuteForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.SecondsPerMinute - 1 );
        return AddSeconds( value - SecondsInMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetSecondsInMinuteForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.SecondsPerMinute + 1, 0 );
        return AddSeconds( value - SecondsInMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetSecondsInMinuteForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.SecondsPerMinute + 1, ChronoConstants.SecondsPerMinute - 1 );
        return AddSeconds( value - SecondsInMinute );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMinutesInHourForPositive(int value)
    {
        Ensure.IsInRange( value, 0, ChronoConstants.MinutesPerHour - 1 );
        return AddMinutes( value - MinutesInHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMinutesInHourForNegative(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MinutesPerHour + 1, 0 );
        return AddMinutes( value - MinutesInHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetMinutesInHourForZero(int value)
    {
        Ensure.IsInRange( value, -ChronoConstants.MinutesPerHour + 1, ChronoConstants.MinutesPerHour - 1 );
        return AddMinutes( value - MinutesInHour );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetHoursForPositive(long value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0 );
        return AddHours( value - FullHours );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetHoursForNegative(long value)
    {
        Ensure.IsLessThanOrEqualTo( value, 0 );
        return AddHours( value - FullHours );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Duration SetHoursForZero(long value)
    {
        return AddHours( value - FullHours );
    }
}
