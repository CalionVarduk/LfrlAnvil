﻿using System;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct Duration : IEquatable<Duration>, IComparable<Duration>, IComparable
    {
        public static readonly Duration Zero = new Duration( 0 );
        public static readonly Duration MinValue = new Duration( long.MinValue );
        public static readonly Duration MaxValue = new Duration( long.MaxValue );

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration(long ticks)
        {
            Ticks = ticks;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration(int hours, int minutes, int seconds)
            : this( hours, minutes, seconds, 0, 0 ) { }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration(int hours, int minutes, int seconds, int milliseconds)
            : this( hours, minutes, seconds, milliseconds, 0 ) { }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration(int hours, int minutes, int seconds, int milliseconds, int ticks)
            : this(
                hours * Constants.TicksPerHour +
                minutes * Constants.TicksPerMinute +
                seconds * Constants.TicksPerSecond +
                milliseconds * Constants.TicksPerMillisecond +
                ticks ) { }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration(TimeSpan timeSpan)
            : this( timeSpan.Ticks ) { }

        public long Ticks { get; }
        public long FullMilliseconds => Ticks / Constants.TicksPerMillisecond;
        public long FullSeconds => Ticks / Constants.TicksPerSecond;
        public long FullMinutes => Ticks / Constants.TicksPerMinute;
        public long FullHours => Ticks / Constants.TicksPerHour;
        public int TicksInMillisecond => (int) (Ticks % Constants.TicksPerMillisecond);
        public int MillisecondsInSecond => (int) (Ticks / Constants.TicksPerMillisecond % Constants.MillisecondsPerSecond);
        public int SecondsInMinute => (int) (Ticks / Constants.TicksPerSecond % Constants.SecondsPerMinute);
        public int MinutesInHour => (int) (Ticks / Constants.TicksPerMinute % Constants.MinutesPerHour);
        public double TotalMilliseconds => (double) Ticks / Constants.TicksPerMillisecond;
        public double TotalSeconds => (double) Ticks / Constants.TicksPerSecond;
        public double TotalMinutes => (double) Ticks / Constants.TicksPerMinute;
        public double TotalHours => (double) Ticks / Constants.TicksPerHour;

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromMilliseconds(double milliseconds)
        {
            return new Duration( (long) Math.Round( milliseconds * Constants.TicksPerMillisecond ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromMilliseconds(long milliseconds)
        {
            return new Duration( milliseconds * Constants.TicksPerMillisecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromSeconds(double seconds)
        {
            return new Duration( (long) Math.Round( seconds * Constants.TicksPerSecond ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromSeconds(long seconds)
        {
            return new Duration( seconds * Constants.TicksPerSecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromMinutes(double minutes)
        {
            return new Duration( (long) Math.Round( minutes * Constants.TicksPerMinute ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromMinutes(long minutes)
        {
            return new Duration( minutes * Constants.TicksPerMinute );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromHours(double hours)
        {
            return new Duration( (long) Math.Round( hours * Constants.TicksPerHour ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration FromHours(long hours)
        {
            return new Duration( hours * Constants.TicksPerHour );
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
        public override bool Equals(object obj)
        {
            return obj is Duration d && Equals( d );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Equals(Duration other)
        {
            return Ticks.Equals( other.Ticks );
        }

        [Pure]
        public int CompareTo(object obj)
        {
            return obj is Duration d ? CompareTo( d ) : 1;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int CompareTo(Duration other)
        {
            return Ticks.CompareTo( other.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration Negate()
        {
            return new Duration( -Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration Add(Duration other)
        {
            return AddTicks( other.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddTicks(long ticks)
        {
            return new Duration( Ticks + ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddMilliseconds(double milliseconds)
        {
            return AddTicks( (long) Math.Round( milliseconds * Constants.TicksPerMillisecond ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddMilliseconds(long milliseconds)
        {
            return AddTicks( milliseconds * Constants.TicksPerMillisecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddSeconds(double seconds)
        {
            return AddTicks( (long) Math.Round( seconds * Constants.TicksPerSecond ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddSeconds(long seconds)
        {
            return AddTicks( seconds * Constants.TicksPerSecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddMinutes(double minutes)
        {
            return AddTicks( (long) Math.Round( minutes * Constants.TicksPerMinute ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddMinutes(long minutes)
        {
            return AddTicks( minutes * Constants.TicksPerMinute );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddHours(double hours)
        {
            return AddTicks( (long) Math.Round( hours * Constants.TicksPerHour ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration AddHours(long hours)
        {
            return AddTicks( hours * Constants.TicksPerHour );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration Subtract(Duration other)
        {
            return SubtractTicks( other.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractTicks(long ticks)
        {
            return AddTicks( -ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractMilliseconds(double milliseconds)
        {
            return SubtractTicks( (long) Math.Round( milliseconds * Constants.TicksPerMillisecond ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractMilliseconds(long milliseconds)
        {
            return SubtractTicks( milliseconds * Constants.TicksPerMillisecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractSeconds(double seconds)
        {
            return SubtractTicks( (long) Math.Round( seconds * Constants.TicksPerSecond ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractSeconds(long seconds)
        {
            return SubtractTicks( seconds * Constants.TicksPerSecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractMinutes(double minutes)
        {
            return SubtractTicks( (long) Math.Round( minutes * Constants.TicksPerMinute ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractMinutes(long minutes)
        {
            return SubtractTicks( minutes * Constants.TicksPerMinute );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractHours(double hours)
        {
            return SubtractTicks( (long) Math.Round( hours * Constants.TicksPerHour ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration SubtractHours(long hours)
        {
            return SubtractTicks( hours * Constants.TicksPerHour );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration TrimToMillisecond()
        {
            return SubtractTicks( Ticks % Constants.TicksPerMillisecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration TrimToSecond()
        {
            return SubtractTicks( Ticks % Constants.TicksPerSecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration TrimToMinute()
        {
            return SubtractTicks( Ticks % Constants.TicksPerMinute );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration TrimToHour()
        {
            return SubtractTicks( Ticks % Constants.TicksPerHour );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator TimeSpan(Duration d)
        {
            return TimeSpan.FromTicks( d.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration operator -(Duration a)
        {
            return a.Negate();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration operator +(Duration a, Duration b)
        {
            return a.Add( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration operator -(Duration a, Duration b)
        {
            return a.Subtract( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(Duration a, Duration b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(Duration a, Duration b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >(Duration a, Duration b)
        {
            return a.CompareTo( b ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <=(Duration a, Duration b)
        {
            return a.CompareTo( b ) <= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <(Duration a, Duration b)
        {
            return a.CompareTo( b ) < 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >=(Duration a, Duration b)
        {
            return a.CompareTo( b ) >= 0;
        }
    }
}
