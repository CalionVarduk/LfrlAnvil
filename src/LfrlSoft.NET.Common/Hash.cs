using LfrlSoft.NET.Common.Internal;
using System;
using System.Collections.Generic;
using System.Linq;

namespace LfrlSoft.NET.Common
{
    public readonly struct Hash : IEquatable<Hash>, IComparable<Hash>, IComparable
    {
        public static readonly int Offset = unchecked( (int) 2166136261 );
        public static readonly int Prime = 16777619;

        public static readonly Hash Null = new Hash( 0 );
        public static readonly Hash Default = new Hash( Offset );

        public readonly int Value;

        public Hash(int value)
        {
            Value = value;
        }

        public override string ToString()
        {
            return $"{nameof( Hash )}({Value})";
        }

        public override int GetHashCode()
        {
            return Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Hash h && Equals( h );
        }

        public bool Equals(Hash other)
        {
            return Value.Equals( other.Value );
        }

        public int CompareTo(object? obj)
        {
            return obj is Hash h ? CompareTo( h ) : 1;
        }

        public int CompareTo(Hash other)
        {
            return Value.CompareTo( other.Value );
        }

        public Hash Add<T>(T? obj)
        {
            return Generic<T>.IsNull( obj )
                ? new Hash( unchecked( (Value ^ Null.Value) * Prime ) )
                : new Hash( unchecked( (Value ^ obj!.GetHashCode()) * Prime ) );
        }

        public Hash AddRange<T>(IEnumerable<T> range)
        {
            var result = new Hash( Value );
            foreach ( var obj in range )
                result = result.Add( obj );

            return result;
        }

        public Hash AddRange<T>(params T[] range)
        {
            return AddRange( range.AsEnumerable() );
        }

        public static implicit operator int(Hash h)
        {
            return h.Value;
        }

        public static bool operator ==(Hash a, Hash b)
        {
            return a.Value == b.Value;
        }

        public static bool operator !=(Hash a, Hash b)
        {
            return a.Value != b.Value;
        }

        public static bool operator >(Hash a, Hash b)
        {
            return a.Value > b.Value;
        }

        public static bool operator <=(Hash a, Hash b)
        {
            return a.Value <= b.Value;
        }

        public static bool operator <(Hash a, Hash b)
        {
            return a.Value < b.Value;
        }

        public static bool operator >=(Hash a, Hash b)
        {
            return a.Value >= b.Value;
        }
    }
}
