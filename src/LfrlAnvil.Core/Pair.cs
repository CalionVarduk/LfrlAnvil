using System;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil
{
    public readonly struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        public readonly T1 First;
        public readonly T2 Second;

        public Pair(T1 first, T2 second)
        {
            First = first;
            Second = second;
        }

        [Pure]
        public override string ToString()
        {
            return $"{nameof( Pair )}({Generic<T1>.ToString( First )}, {Generic<T2>.ToString( Second )})";
        }

        [Pure]
        public override int GetHashCode()
        {
            return Hash.Default
                .Add( First )
                .Add( Second )
                .Value;
        }

        [Pure]
        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> p && Equals( p );
        }

        [Pure]
        public bool Equals(Pair<T1, T2> other)
        {
            return Equality.Create( First, other.First ).Result &&
                Equality.Create( Second, other.Second ).Result;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Pair<T, T2> SetFirst<T>(T first)
        {
            return new Pair<T, T2>( first, Second );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public Pair<T1, T> SetSecond<T>(T second)
        {
            return new Pair<T1, T>( First, second );
        }

        [Pure]
        public static bool operator ==(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return a.Equals( b );
        }

        [Pure]
        public static bool operator !=(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return ! a.Equals( b );
        }
    }
}
