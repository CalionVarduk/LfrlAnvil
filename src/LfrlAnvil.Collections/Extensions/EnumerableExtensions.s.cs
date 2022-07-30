using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Extensions;

public static class EnumerableExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiHashSet<T> ToMultiHashSet<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.ToMultiHashSet( EqualityComparer<T>.Default );
    }

    [Pure]
    public static MultiHashSet<T> ToMultiHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        where T : notnull
    {
        var result = new MultiHashSet<T>( comparer );
        foreach ( var e in source )
            result.Add( e );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector)
        where TKey : notnull
    {
        return source.ToMultiDictionary( keySelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var result = new MultiDictionary<TKey, TValue>( comparer );
        foreach ( var value in source )
            result.Add( keySelector( value ), value );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector)
        where TKey : notnull
    {
        return source.ToMultiDictionary( keySelector, valueSelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var result = new MultiDictionary<TKey, TValue>( comparer );
        foreach ( var e in source )
            result.Add( keySelector( e ), valueSelector( e ) );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        where TKey : notnull
    {
        return source.ToMultiDictionary( EqualityComparer<TKey>.Default );
    }

    [Pure]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(
        this IEnumerable<KeyValuePair<TKey, TValue>> source,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var result = new MultiDictionary<TKey, TValue>( comparer );
        foreach ( var (key, value) in source )
            result.Add( key, value );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> source)
        where TKey : notnull
    {
        return source.ToMultiDictionary( EqualityComparer<TKey>.Default );
    }

    [Pure]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(
        this IEnumerable<IGrouping<TKey, TValue>> source,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var result = new MultiDictionary<TKey, TValue>( comparer );
        foreach ( var g in source )
            result.AddRange( g.Key, g );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<KeyValuePair<TKey, IReadOnlyList<TValue>>> AsEnumerable<TKey, TValue>(
        this IReadOnlyMultiDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        return source;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SequentialHashSet<T> ToSequentialHashSet<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.ToSequentialHashSet( EqualityComparer<T>.Default );
    }

    [Pure]
    public static SequentialHashSet<T> ToSequentialHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        where T : notnull
    {
        var result = new SequentialHashSet<T>( comparer );
        foreach ( var e in source )
            result.Add( e );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SequentialDictionary<TKey, TSource> ToSequentialDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
        where TKey : notnull
    {
        return source.ToSequentialDictionary( keySelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    public static SequentialDictionary<TKey, TSource> ToSequentialDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var result = new SequentialDictionary<TKey, TSource>( comparer );
        foreach ( var e in source )
            result.Add( keySelector( e ), e );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SequentialDictionary<TKey, TValue> ToSequentialDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector)
        where TKey : notnull
    {
        return source.ToSequentialDictionary( keySelector, valueSelector, EqualityComparer<TKey>.Default );
    }

    [Pure]
    public static SequentialDictionary<TKey, TValue> ToSequentialDictionary<TSource, TKey, TValue>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector,
        Func<TSource, TValue> valueSelector,
        IEqualityComparer<TKey> comparer)
        where TKey : notnull
    {
        var result = new SequentialDictionary<TKey, TValue>( comparer );
        foreach ( var e in source )
            result.Add( keySelector( e ), valueSelector( e ) );

        return result;
    }
}
