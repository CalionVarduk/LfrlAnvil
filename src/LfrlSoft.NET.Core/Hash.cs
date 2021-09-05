using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core
{
    public readonly struct Hash : IEquatable<Hash>, IComparable<Hash>, IComparable
    {
        public const int Offset = unchecked( (int) 2166136261 );
        public const int Prime = 16777619;

        public static readonly Hash Null = new Hash( 0 );
        public static readonly Hash Default = new Hash( Offset );

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
            return Generic<T>.IsNull( obj )
                ? new Hash( unchecked( (Value ^ Null.Value) * Prime ) )
                : new Hash( unchecked( (Value ^ obj!.GetHashCode()) * Prime ) );
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
            return AddRange( range.AsEnumerable() );
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
