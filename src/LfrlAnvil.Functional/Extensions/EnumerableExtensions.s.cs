using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains <see cref="IEnumerable{T}"/> extension methods.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Filters out <see cref="Maybe{T}.None"/> elements from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with <see cref="Maybe{T}.None"/> elements filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> SelectValues<T>(this IEnumerable<Maybe<T>> source)
        where T : notnull
    {
        return source.Where( e => e.HasValue ).Select( e => e.Value! );
    }

    /// <summary>
    /// Filters out elements with second value from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with elements with second value filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T1> SelectFirst<T1, T2>(this IEnumerable<Either<T1, T2>> source)
    {
        return source.Where( e => e.HasFirst ).Select( e => e.First! );
    }

    /// <summary>
    /// Filters out elements with first value from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with elements with first value filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T2> SelectSecond<T1, T2>(this IEnumerable<Either<T1, T2>> source)
    {
        return source.Where( e => e.HasSecond ).Select( e => e.Second! );
    }

    /// <summary>
    /// Partitions the provided <paramref name="source"/> into separate collections that contain first and second values.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T1">First either type.</typeparam>
    /// <typeparam name="T2">Second either type.</typeparam>
    /// <returns>New tuple that contains partitioning result.</returns>
    [Pure]
    public static (List<T1> First, List<T2> Second) Partition<T1, T2>(this IEnumerable<Either<T1, T2>> source)
    {
        var first = new List<T1>();
        var second = new List<T2>();

        foreach ( var e in source )
        {
            if ( e.HasFirst )
                first.Add( e.First );
            else
                second.Add( e.Second );
        }

        return (first, second);
    }

    /// <summary>
    /// Filters out elements with errors from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with elements with errors filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> SelectValues<T>(this IEnumerable<Erratic<T>> source)
    {
        return source.Where( e => e.IsOk ).Select( e => e.Value! );
    }

    /// <summary>
    /// Filters out elements with values from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with elements with values filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<Exception> SelectErrors<T>(this IEnumerable<Erratic<T>> source)
    {
        return source.Where( e => e.HasError ).Select( e => e.Error! );
    }

    /// <summary>
    /// Partitions the provided <paramref name="source"/> into separate collections that contain values and errors.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New tuple that contains partitioning result.</returns>
    [Pure]
    public static (List<T> Values, List<Exception> Errors) Partition<T>(this IEnumerable<Erratic<T>> source)
    {
        var values = new List<T>();
        var errors = new List<Exception>();

        foreach ( var e in source )
        {
            if ( e.IsOk )
                values.Add( e.Value );
            else
                errors.Add( e.Error );
        }

        return (values, errors);
    }

    /// <summary>
    /// Attempts to find the minimum value in the provided <paramref name="source"/>
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMin<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.TryMin( Comparer<T>.Default );
    }

    /// <summary>
    /// Attempts to find the minimum value in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMin<T>(this IEnumerable<T> source, IComparer<T> comparer)
        where T : notnull
    {
        return source.TryMin( comparer, out var result ) ? new Maybe<T>( result ) : Maybe<T>.None;
    }

    /// <summary>
    /// Attempts to find the maximum value in the provided <paramref name="source"/>
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMax<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.TryMax( Comparer<T>.Default );
    }

    /// <summary>
    /// Attempts to find the maximum value in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryMax<T>(this IEnumerable<T> source, IComparer<T> comparer)
        where T : notnull
    {
        return source.TryMax( comparer, out var result ) ? new Maybe<T>( result ) : Maybe<T>.None;
    }

    /// <summary>
    /// Attempts to compute an aggregation for the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="func">Aggregator delegate.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the aggregation result
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T> TryAggregate<T>(this IEnumerable<T> source, Func<T, T, T> func)
        where T : notnull
    {
        return source.TryAggregate( func, out var result ) ? new Maybe<T>( result ) : Maybe<T>.None;
    }

    /// <summary>
    /// Attempts to find an element with the maximum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/> using the <see cref="Comparer{T2}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        where T1 : notnull
    {
        return source.TryMaxBy( selector, Comparer<T2>.Default );
    }

    /// <summary>
    /// Attempts to find an element with the maximum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
        where T1 : notnull
    {
        return source.TryMaxBy( selector, comparer, out var result ) ? new Maybe<T1>( result ) : Maybe<T1>.None;
    }

    /// <summary>
    /// Attempts to find an element with the minimum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/> using the <see cref="Comparer{T2}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
        where T1 : notnull
    {
        return source.TryMinBy( selector, Comparer<T2>.Default );
    }

    /// <summary>
    /// Attempts to find an element with the minimum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the found value
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<T1> TryMinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
        where T1 : notnull
    {
        return source.TryMinBy( selector, comparer, out var result ) ? new Maybe<T1>( result ) : Maybe<T1>.None;
    }

    /// <summary>
    /// Attempts to get the first element in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the first element
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    public static Maybe<T> TryFirst<T>(this IEnumerable<T> source)
        where T : notnull
    {
        if ( source is IReadOnlyList<T> list )
            return list.Count > 0 ? new Maybe<T>( list[0] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();
        return enumerator.MoveNext() ? new Maybe<T>( enumerator.Current ) : Maybe<T>.None;
    }

    /// <summary>
    /// Attempts to get the last element in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the last element
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    public static Maybe<T> TryLast<T>(this IEnumerable<T> source)
        where T : notnull
    {
        if ( source is IReadOnlyList<T> list )
            return list.Count > 0 ? new Maybe<T>( list[^1] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return Maybe<T>.None;

        var last = enumerator.Current;
        while ( enumerator.MoveNext() )
            last = enumerator.Current;

        return new Maybe<T>( last );
    }

    /// <summary>
    /// Attempts to get the only element in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the only element
    /// or <see cref="Maybe{T}.None"/> when <paramref name="source"/> is empty or contains more than <b>1</b> element.
    /// </returns>
    [Pure]
    public static Maybe<T> TrySingle<T>(this IEnumerable<T> source)
        where T : notnull
    {
        if ( source is IReadOnlyList<T> list )
            return list.Count == 1 ? new Maybe<T>( list[0] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return Maybe<T>.None;

        var candidate = enumerator.Current;
        return enumerator.MoveNext() ? Maybe<T>.None : new Maybe<T>( candidate );
    }

    /// <summary>
    /// Attempts to get an element at the specified <paramref name="index"/> in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="index">0-based position of an element to find.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to the element at the specified position
    /// or <see cref="Maybe{T}.None"/> when <paramref name="index"/> is not in [<b>0</b>, <paramref name="source"/> count) range.
    /// </returns>
    [Pure]
    public static Maybe<T> TryElementAt<T>(this IEnumerable<T> source, int index)
        where T : notnull
    {
        if ( index < 0 )
            return Maybe<T>.None;

        if ( source is IReadOnlyList<T> list )
            return index < list.Count ? new Maybe<T>( list[index] ) : Maybe<T>.None;

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return Maybe<T>.None;

        if ( index == 0 )
            return new Maybe<T>( enumerator.Current );

        --index;
        while ( enumerator.MoveNext() )
        {
            if ( index == 0 )
                return new Maybe<T>( enumerator.Current );

            --index;
        }

        return Maybe<T>.None;
    }
}
