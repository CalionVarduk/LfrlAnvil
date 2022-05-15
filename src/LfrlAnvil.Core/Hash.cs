using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil
{
    public readonly struct Hash : IEquatable<Hash>, IComparable<Hash>, IComparable
    {
        public static readonly Hash Default = new Hash( HashCode.Combine( 0 ) );

        public readonly int Value;

        public Hash(int value)
        {
            Value = value;
        }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Hash )}({Value})";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public override int GetHashCode()
        {
            return Value;
        }

        [Pure]
        public override bool Equals(object? obj)
        {
            return obj is Hash h && Equals( h );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool Equals(Hash other)
        {
            return Value.Equals( other.Value );
        }

        [Pure]
        public int CompareTo(object? obj)
        {
            return obj is Hash h ? CompareTo( h ) : 1;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public int CompareTo(Hash other)
        {
            return Value.CompareTo( other.Value );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Hash Add<T>(T? obj)
        {
            return new Hash( HashCode.Combine( Value, obj ) );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Hash AddRange<T>(IEnumerable<T?> range)
        {
            var result = new Hash( Value );
            foreach ( var obj in range )
                result = result.Add( obj );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Hash AddRange<T>(params T?[] range)
        {
            var result = new Hash( Value );
            foreach ( var obj in range )
                result = result.Add( obj );

            return result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static implicit operator int(Hash h)
        {
            return h.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator ==(Hash a, Hash b)
        {
            return a.Value == b.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator !=(Hash a, Hash b)
        {
            return a.Value != b.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >(Hash a, Hash b)
        {
            return a.Value > b.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <=(Hash a, Hash b)
        {
            return a.Value <= b.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator <(Hash a, Hash b)
        {
            return a.Value < b.Value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public static bool operator >=(Hash a, Hash b)
        {
            return a.Value >= b.Value;
        }
    }
}
