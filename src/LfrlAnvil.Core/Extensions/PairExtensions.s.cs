using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

public static class PairExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Pair<T1, T2> ToPair<T1, T2>(this Tuple<T1, T2> source)
    {
        return Pair.Create( source.Item1, source.Item2 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Pair<T1, T2> ToPair<T1, T2>(this ValueTuple<T1, T2> source)
    {
        return Pair.Create( source.Item1, source.Item2 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Tuple<T1, T2> ToTuple<T1, T2>(this Pair<T1, T2> source)
    {
        return Tuple.Create( source.First, source.Second );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTuple<T1, T2> ToValueTuple<T1, T2>(this Pair<T1, T2> source)
    {
        return ValueTuple.Create( source.First, source.Second );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T, T> source)
    {
        yield return source.First;
        yield return source.Second;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T, T?> source)
        where T : struct
    {
        yield return source.First;

        if ( source.Second.HasValue )
            yield return source.Second.Value;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T?, T> source)
        where T : struct
    {
        if ( source.First.HasValue )
            yield return source.First.Value;

        yield return source.Second;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T?, T?> source)
        where T : struct
    {
        if ( source.First.HasValue )
            yield return source.First.Value;

        if ( source.Second.HasValue )
            yield return source.Second.Value;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Deconstruct<T1, T2>(this Pair<T1, T2> source, out T1 first, out T2 second)
    {
        first = source.First;
        second = source.Second;
    }
}
