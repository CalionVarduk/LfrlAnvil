using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct TimeOfDay : IEquatable<TimeOfDay>, IComparable<TimeOfDay>, IComparable
    {
        public static readonly TimeOfDay Start = new TimeOfDay( 0 );
        public static readonly TimeOfDay Mid = new TimeOfDay( (Constants.HoursPerDay >> 1) * Constants.TicksPerHour );
        public static readonly TimeOfDay End = new TimeOfDay( Constants.TicksPerDay - 1 );

        private readonly long _value;

        public TimeOfDay(int hour)
            : this( hour, 0, 0, 0, 0 ) { }

        public TimeOfDay(int hour, int minute)
            : this( hour, minute, 0, 0, 0 ) { }

        public TimeOfDay(int hour, int minute, int second)
            : this( hour, minute, second, 0, 0 ) { }

        public TimeOfDay(int hour, int minute, int second, int millisecond)
            : this( hour, minute, second, millisecond, 0 ) { }

        public TimeOfDay(int hour, int minute, int second, int millisecond, int tick)
        {
            Ensure.IsInRange( hour, 0, Constants.HoursPerDay - 1, nameof( hour ) );
            Ensure.IsInRange( minute, 0, Constants.MinutesPerHour - 1, nameof( minute ) );
            Ensure.IsInRange( second, 0, Constants.SecondsPerMinute - 1, nameof( second ) );
            Ensure.IsInRange( millisecond, 0, Constants.MillisecondsPerSecond - 1, nameof( millisecond ) );
            Ensure.IsInRange( tick, 0, Constants.TicksPerMillisecond - 1, nameof( tick ) );

            _value = hour * Constants.TicksPerHour +
                minute * Constants.TicksPerMinute +
                second * Constants.TicksPerSecond +
                millisecond * Constants.TicksPerMillisecond +
                tick;
        }

        public TimeOfDay(TimeSpan timeSpan)
            : this(
                (int)timeSpan.TotalHours,
                timeSpan.Minutes,
                timeSpan.Seconds,
                timeSpan.Milliseconds,
                (int)(timeSpan.Ticks % Constants.TicksPerMillisecond) ) { }

        private TimeOfDay(long value)
        {
            _value = value;
        }

        public int Tick => (int)(_value % Constants.TicksPerMillisecond);
        public int Millisecond => (int)(_value / Constants.TicksPerMillisecond % Constants.MillisecondsPerSecond);
        public int Second => (int)(_value / Constants.TicksPerSecond % Constants.SecondsPerMinute);
        public int Minute => (int)(_value / Constants.TicksPerMinute % Constants.MinutesPerHour);
        public int Hour => (int)(_value / Constants.TicksPerHour);

        [Pure]
        public override string ToString()
        {
            return $"{Hour:00}h {Minute:00}m {Second:00}.{Millisecond:000}{Tick:0000}s";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override int GetHashCode()
        {
            return _value.GetHashCode();
        }

        [Pure]
        public override bool Equals(object obj)
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
        public int CompareTo(object obj)
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
            return _value == 0 ? this : new TimeOfDay( Constants.TicksPerDay - _value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration Subtract(TimeOfDay other)
        {
            return Duration.FromTicks( _value - other._value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay TrimToMillisecond()
        {
            return new TimeOfDay( _value - _value % Constants.TicksPerMillisecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay TrimToSecond()
        {
            return new TimeOfDay( _value - _value % Constants.TicksPerSecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay TrimToMinute()
        {
            return new TimeOfDay( _value - _value % Constants.TicksPerMinute );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay TrimToHour()
        {
            return new TimeOfDay( _value - _value % Constants.TicksPerHour );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay SetTick(int value)
        {
            Ensure.IsInRange( value, 0, Constants.TicksPerMillisecond - 1, nameof( value ) );
            return new TimeOfDay( _value + value - Tick );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay SetMillisecond(int value)
        {
            Ensure.IsInRange( value, 0, Constants.MillisecondsPerSecond - 1, nameof( value ) );
            return new TimeOfDay( _value + (value - Millisecond) * Constants.TicksPerMillisecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay SetSecond(int value)
        {
            Ensure.IsInRange( value, 0, Constants.SecondsPerMinute - 1, nameof( value ) );
            return new TimeOfDay( _value + (value - Second) * Constants.TicksPerSecond );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay SetMinute(int value)
        {
            Ensure.IsInRange( value, 0, Constants.MinutesPerHour - 1, nameof( value ) );
            return new TimeOfDay( _value + (value - Minute) * Constants.TicksPerMinute );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public TimeOfDay SetHour(int value)
        {
            Ensure.IsInRange( value, 0, Constants.HoursPerDay - 1, nameof( value ) );
            return new TimeOfDay( _value + (value - Hour) * Constants.TicksPerHour );
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
}
