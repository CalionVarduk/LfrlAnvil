using System;
using LfrlSoft.NET.Common.Internal;

namespace LfrlSoft.NET.Common
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

        public override string ToString()
        {
            return $"{nameof( Pair )}({Generic<T1>.ToString( First )}, {Generic<T2>.ToString( Second )})";
        }

        public override int GetHashCode()
        {
            return Hash.Default
                .Add( First )
                .Add( Second )
                .Value;
        }

        public override bool Equals(object obj)
        {
            return obj is Pair<T1, T2> p && Equals( p );
        }

        public bool Equals(Pair<T1, T2> other)
        {
            return Equality.Create( First, other.First ).Result &&
                Equality.Create( Second, other.Second ).Result;
        }

        public Pair<T, T2> SetFirst<T>(T first)
        {
            return new Pair<T, T2>( first, Second );
        }

        public Pair<T1, T> SetSecond<T>(T second)
        {
            return new Pair<T1, T>( First, second );
        }


        public static bool operator ==(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return a.Equals( b );
        }

        public static bool operator !=(Pair<T1, T2> a, Pair<T1, T2> b)
        {
            return ! a.Equals( b );
        }
    }
}
