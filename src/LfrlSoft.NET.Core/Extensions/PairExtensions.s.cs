using System;
using System.Diagnostics.Contracts;

namespace LfrlSoft.NET.Core.Extensions
{
    public static class PairExtensions
    {
        [Pure]
        public static Pair<T1, T2> ToPair<T1, T2>(this Tuple<T1, T2> source)
        {
            return Pair.Create( source.Item1, source.Item2 );
        }

        [Pure]
        public static Pair<T1, T2> ToPair<T1, T2>(this ValueTuple<T1, T2> source)
        {
            return Pair.Create( source.Item1, source.Item2 );
        }

        [Pure]
        public static Tuple<T1, T2> ToTuple<T1, T2>(this Pair<T1, T2> source)
        {
            return Tuple.Create( source.First, source.Second );
        }

        [Pure]
        public static ValueTuple<T1, T2> ToValueTuple<T1, T2>(this Pair<T1, T2> source)
        {
            return ValueTuple.Create( source.First, source.Second );
        }

        public static void Deconstruct<T1, T2>(this Pair<T1, T2> source, out T1 first, out T2 second)
        {
            first = source.First;
            second = source.Second;
        }
    }
}
