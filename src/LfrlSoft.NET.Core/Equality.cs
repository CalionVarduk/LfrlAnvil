using System;
using LfrlSoft.NET.Core.Internal;

namespace LfrlSoft.NET.Core
{
    public readonly struct Equality<T> : IEquatable<Equality<T>>
    {
        public readonly T? First;
        public readonly T? Second;
        public readonly bool Result;

        public Equality(T? first, T? second)
        {
            First = first;
            Second = second;
            Result = Generic<T>.AreEqual( First, Second );
        }

        public override string ToString()
        {
            return $"{nameof( Equality )}({Generic<T>.ToString( First )}, {Generic<T>.ToString( Second )})";
        }

        public override int GetHashCode()
        {
            return Hash.Default
                .Add( First )
                .Add( Second )
                .Add( Result )
                .Value;
        }

        public override bool Equals(object? obj)
        {
            return obj is Equality<T> e && Equals( e );
        }

        public bool Equals(Equality<T> other)
        {
            return Generic<T>.AreEqual( First, other.First ) &&
                Generic<T>.AreEqual( Second, other.Second ) &&
                Result == other.Result;
        }

        public static implicit operator bool(Equality<T> e)
        {
            return e.Result;
        }

        public static bool operator !(Equality<T> e)
        {
            return ! e.Result;
        }

        public static bool operator ==(Equality<T> a, Equality<T> b)
        {
            return a.Equals( b );
        }

        public static bool operator !=(Equality<T> a, Equality<T> b)
        {
            return ! a.Equals( b );
        }
    }
}
