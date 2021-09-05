using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlSoft.NET.Core.Chrono
{
    public readonly struct Timestamp : IEquatable<Timestamp>, IComparable<Timestamp>, IComparable
    {
        public static readonly Timestamp Zero = new Timestamp( DateTime.UnixEpoch );

        public Timestamp(long unixEpochTicks)
            : this( DateTime.UnixEpoch.AddTicks( unixEpochTicks ) ) { }

        public Timestamp(DateTime utcValue)
        {
            UtcValue = DateTime.SpecifyKind( utcValue, DateTimeKind.Utc );
            UnixEpochTicks = UtcValue.Ticks - DateTime.UnixEpoch.Ticks;
        }

        public long UnixEpochTicks { get; }
        public DateTime UtcValue { get; }

        [Pure]
        public override string ToString()
        {
            return $"{UnixEpochTicks} ticks";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override int GetHashCode()
        {
            return UnixEpochTicks.GetHashCode();
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is Timestamp t && Equals( t );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Equals(Timestamp other)
        {
            return UnixEpochTicks.Equals( other.UnixEpochTicks );
        }

        [Pure]
        public int CompareTo(object obj)
        {
            return obj is Timestamp t ? CompareTo( t ) : 1;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int CompareTo(Timestamp other)
        {
            return UnixEpochTicks.CompareTo( other.UnixEpochTicks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Timestamp Add(Duration value)
        {
            return new Timestamp( UnixEpochTicks + value.Ticks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Timestamp Subtract(Duration value)
        {
            return Add( -value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Duration Subtract(Timestamp other)
        {
            return Duration.FromTicks( UnixEpochTicks - other.UnixEpochTicks );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static explicit operator DateTime(Timestamp source)
        {
            return source.UtcValue;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Timestamp operator +(Timestamp a, Duration b)
        {
            return a.Add( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Timestamp operator -(Timestamp a, Duration b)
        {
            return a.Subtract( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static Duration operator -(Timestamp a, Timestamp b)
        {
            return a.Subtract( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(Timestamp a, Timestamp b)
        {
            return a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(Timestamp a, Timestamp b)
        {
            return ! a.Equals( b );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >(Timestamp a, Timestamp b)
        {
            return a.CompareTo( b ) > 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <=(Timestamp a, Timestamp b)
        {
            return a.CompareTo( b ) <= 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <(Timestamp a, Timestamp b)
        {
            return a.CompareTo( b ) < 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >=(Timestamp a, Timestamp b)
        {
            return a.CompareTo( b ) >= 0;
        }
    }
}
