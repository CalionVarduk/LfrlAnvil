// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="IEnumerable{T}"/> extension methods.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Returns the provided <paramref name="source"/>, unless it is null, in which case returns an empty enumerable instead.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><paramref name="source"/> if it is not null, otherwise an empty enumerable.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> EmptyIfNull<T>(this IEnumerable<T>? source)
    {
        return source ?? Enumerable.Empty<T>();
    }

    /// <summary>
    /// Filters out null elements from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with null elements filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Where( static e => e is not null )!;
    }

    /// <summary>
    /// Filters out null elements from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> with null elements filtered out.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source)
        where T : struct
    {
        return source.Where( static e => e.HasValue ).Select( static e => e!.Value );
    }

    /// <summary>
    /// Filters out null elements from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Element equality comparer.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> with null elements filtered out,
    /// or <paramref name="source"/> when element type is a non-nullable value type.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> WhereNotNull<T>(this IEnumerable<T?> source, IEqualityComparer<T> comparer)
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            return source!;

        return source.Where( e => ! comparer.Equals( e, default ) )!;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> contains at least one null element.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains at least one null element, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsNull<T>(this IEnumerable<T?> source)
        where T : class
    {
        return source.Any( static e => e is null );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> contains at least one null element.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains at least one null element, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsNull<T>(this IEnumerable<T?> source)
        where T : struct
    {
        return source.Any( static e => ! e.HasValue );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> contains at least one null element.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Element equality comparer.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains at least one null element, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsNull<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
    {
        if ( typeof( T ).IsValueType && ! Generic<T>.IsNullableType )
            return false;

        return source.Any( e => comparer.Equals( e, default ) );
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> is null or empty.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is null or empty, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsNullOrEmpty<T>([NotNullWhen( false )] this IEnumerable<T>? source)
    {
        return source is null || source.IsEmpty();
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> is empty.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is empty, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsEmpty<T>(this IEnumerable<T> source)
    {
        return ! source.Any();
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains at least <paramref name="count"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Expected minimum number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtLeast<T>(this IEnumerable<T> source, int count)
    {
        if ( count <= 0 )
            return true;

        if ( source.TryGetNonEnumeratedCount( out var counter ) )
            return counter >= count;

        return source.Skip( count - 1 ).Any();
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains at most <paramref name="count"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Expected maximum number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsAtMost<T>(this IEnumerable<T> source, int count)
    {
        if ( count < 0 )
            return false;

        if ( source.TryGetNonEnumeratedCount( out var counter ) )
            return counter <= count;

        return ! source.Skip( count ).Any();
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains between
    /// <paramref name="minCount"/> and <paramref name="maxCount"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="minCount">Expected minimum number of elements.</param>
    /// <param name="maxCount">Expected maximum number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool ContainsInRange<T>(this IEnumerable<T> source, int minCount, int maxCount)
    {
        if ( maxCount < minCount )
            return false;

        if ( minCount <= 0 )
            return source.ContainsAtMost( maxCount );

        if ( source.TryGetNonEnumeratedCount( out var counter ) )
            return counter >= minCount && counter <= maxCount;

        using var enumerator = source.Skip( minCount - 1 ).GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return false;

        counter = 1;
        var counterLimit = maxCount - minCount + 1;

        while ( enumerator.MoveNext() )
        {
            if ( ++counter > counterLimit )
                return false;
        }

        return true;
    }

    /// <summary>
    /// Checks if the provided <paramref name="source"/> contains exactly <paramref name="count"/> number of elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Expected exact number of elements.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains correct number of elements, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool ContainsExactly<T>(this IEnumerable<T> source, int count)
    {
        if ( count < 0 )
            return false;

        if ( source.TryGetNonEnumeratedCount( out var counter ) )
            return counter == count;

        using var enumerator = source.GetEnumerator();

        while ( enumerator.MoveNext() )
        {
            if ( ++counter > count )
                break;
        }

        return counter == count;
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that converts <paramref name="source"/> elements to <see cref="Nullable{T}"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T?> AsNullable<T>(this IEnumerable<T> source)
        where T : struct
    {
        return source.Select( static e => ( T? )e );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains a collection of (parent, child) pairs.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Child selector.</param>
    /// <typeparam name="T1">Source collection element (parent) type.</typeparam>
    /// <typeparam name="T2">Child type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<Pair<T1, T2>> Flatten<T1, T2>(this IEnumerable<T1> source, Func<T1, IEnumerable<T2>> selector)
    {
        return source.Flatten( selector, Pair.Create );
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains a collection of (parent, child) pairs mapped to the desired type.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Child selector.</param>
    /// <param name="resultMapper">Result selector.</param>
    /// <typeparam name="T1">Source collection element (parent) type.</typeparam>
    /// <typeparam name="T2">Child type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TResult> Flatten<T1, T2, TResult>(
        this IEnumerable<T1> source,
        Func<T1, IEnumerable<T2>> selector,
        Func<T1, T2, TResult> resultMapper)
    {
        foreach ( var p in source )
        {
            foreach ( var c in selector( p ) )
                yield return resultMapper( p, c );
        }
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all elements from nested collections.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns></returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> Flatten<T>(this IEnumerable<IEnumerable<T>> source)
    {
        return source.SelectMany( static x => x );
    }

    /// <summary>
    /// Attempts to find the minimum value in the provided <paramref name="source"/>
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="result"><b>out</b> parameter that contains the minimum value, if the collection is not empty.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMin<T>(this IEnumerable<T> source, [MaybeNullWhen( false )] out T result)
    {
        return source.TryMin( Comparer<T>.Default, out result );
    }

    /// <summary>
    /// Attempts to find the minimum value in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <param name="result"><b>out</b> parameter that contains the minimum value, if the collection is not empty.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMin<T>(this IEnumerable<T> source, IComparer<T> comparer, [MaybeNullWhen( false )] out T result)
    {
        return source.TryAggregate( (a, b) => comparer.Compare( a, b ) < 0 ? a : b, out result );
    }

    /// <summary>
    /// Attempts to find the maximum value in the provided <paramref name="source"/>
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="result"><b>out</b> parameter that contains the maximum value, if the collection is not empty.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMax<T>(this IEnumerable<T> source, [MaybeNullWhen( false )] out T result)
    {
        return source.TryMax( Comparer<T>.Default, out result );
    }

    /// <summary>
    /// Attempts to find the maximum value in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <param name="result"><b>out</b> parameter that contains the maximum value, if the collection is not empty.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMax<T>(this IEnumerable<T> source, IComparer<T> comparer, [MaybeNullWhen( false )] out T result)
    {
        return source.TryAggregate( (a, b) => comparer.Compare( a, b ) > 0 ? a : b, out result );
    }

    /// <summary>
    /// Finds the minimum and maximum value in the provided <paramref name="source"/>
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>A tuple containing the <b>Min</b> and <b>Max</b> values.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="source"/> is empty.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source)
    {
        return source.MinMax( Comparer<T>.Default );
    }

    /// <summary>
    /// Finds the minimum and maximum value in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>A tuple containing the <b>Min</b> and <b>Max</b> values.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="source"/> is empty.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T Min, T Max) MinMax<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        var result = source.TryMinMax( comparer );
        if ( result is null )
            ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.SequenceContainsNoElements ) );

        return result.Value;
    }

    /// <summary>
    /// Attempts to find the minimum and maximum value in the provided <paramref name="source"/>
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>A tuple containing the <b>Min</b> and <b>Max</b> values, or null when <paramref name="source"/> is empty.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T Min, T Max)? TryMinMax<T>(this IEnumerable<T> source)
    {
        return source.TryMinMax( Comparer<T>.Default );
    }

    /// <summary>
    /// Attempts to find the minimum and maximum value in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>A tuple containing the <b>Min</b> and <b>Max</b> values, or null when <paramref name="source"/> is empty.</returns>
    [Pure]
    public static (T Min, T Max)? TryMinMax<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return null;

        var min = enumerator.Current;
        var max = min;

        while ( enumerator.MoveNext() )
        {
            var current = enumerator.Current;

            if ( comparer.Compare( min, current ) > 0 )
                min = current;
            else if ( comparer.Compare( max, current ) < 0 )
                max = current;
        }

        return (min, max);
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> contains duplicated elements,
    /// using the <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains at least one duplicated element, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool ContainsDuplicates<T>(this IEnumerable<T> source)
    {
        return source.ContainsDuplicates( EqualityComparer<T>.Default );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> contains duplicated elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for element equality.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> contains at least one duplicated element, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool ContainsDuplicates<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
    {
        var set = new HashSet<T>( comparer );

        foreach ( var e in source )
        {
            if ( ! set.Add( e ) )
                return true;
        }

        return false;
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that contains the <paramref name="source"/>
    /// repeated <paramref name="count"/> times.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="count">Number of <paramref name="source"/> repetitions.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance,
    /// or empty enumerable when <paramref name="count"/> is equal to <b>0</b>,
    /// or <paramref name="source"/> when <paramref name="count"/> is equal to <b>1</b>
    /// .</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>0</b>.</exception>
    [Pure]
    public static IEnumerable<T> Repeat<T>(this IEnumerable<T> source, int count)
    {
        if ( count == 0 )
            return Enumerable.Empty<T>();

        if ( count == 1 )
            return source;

        Ensure.IsGreaterThanOrEqualTo( count, 0 );

        var memoizedSource = source.Memoize();

        var result = memoizedSource.Concat( memoizedSource );
        for ( var i = 2; i < count; ++i )
            result = result.Concat( memoizedSource );

        return result;
    }

    /// <summary>
    /// Materialized the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance,
    /// or <paramref name="source"/> when it is an instance of <see cref="IReadOnlyCollection{T}"/>,
    /// or memoized value when <paramref name="source"/> is an instance of <see cref="IMemoizedCollection{T}"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IReadOnlyCollection<T> Materialize<T>(this IEnumerable<T> source)
    {
        return source switch
        {
            IMemoizedCollection<T> m => m.Source.Value,
            IReadOnlyCollection<T> c => c,
            _ => source.ToList()
        };
    }

    /// <summary>
    /// Memoizes the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IMemoizedCollection{T}"/> instance,
    /// or <paramref name="source"/> when it is an instance of <see cref="IMemoizedCollection{T}"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IMemoizedCollection<T> Memoize<T>(this IEnumerable<T> source)
    {
        return DynamicCast.TryTo<IMemoizedCollection<T>>( source ) ?? new MemoizedCollection<T>( source );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> is either an instance of <see cref="IReadOnlyCollection{T}"/>
    /// or of <see cref="IMemoizedCollection{T}"/> with <see cref="IMemoizedCollection{T}.IsMaterialized"/> set to <b>true</b>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is considered to be materialized, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsMaterialized<T>(this IEnumerable<T> source)
    {
        if ( source is IMemoizedCollection<T> memoized )
            return memoized.IsMaterialized;

        return source is IReadOnlyCollection<T>;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> is an instance of <see cref="IMemoizedCollection{T}"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is considered to be memoized, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsMemoized<T>(this IEnumerable<T> source)
    {
        return source is IMemoizedCollection<T>;
    }

    /// <summary>
    /// Checks whether or not the two collections are considered to be equal sets,
    /// using the <see cref="EqualityComparer{T}.Default"/> equality comparer.
    /// </summary>
    /// <param name="source">First collection.</param>
    /// <param name="other">Second collection.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns><b>true</b> when the two collections are equivalent sets, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other)
    {
        return source.SetEquals( other, EqualityComparer<T>.Default );
    }

    /// <summary>
    /// Checks whether or not the two collections are considered to be equal sets.
    /// </summary>
    /// <param name="source">First collection.</param>
    /// <param name="other">Second collection.</param>
    /// <param name="comparer">Comparer to use for element comparison.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns><b>true</b> when the two collections are equivalent sets, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool SetEquals<T>(this IEnumerable<T> source, IEnumerable<T> other, IEqualityComparer<T> comparer)
    {
        var sourceSet = GetSet( source, comparer );

        if ( other is HashSet<T> otherSet && otherSet.Comparer.Equals( comparer ) )
            return SetEquals( sourceSet, otherSet );

        otherSet = new HashSet<T>( comparer );

        foreach ( var o in other )
        {
            if ( ! sourceSet.Contains( o ) )
                return false;

            otherSet.Add( o );
        }

        return sourceSet.Count == otherSet.Count;
    }

    /// <summary>
    /// Recursively visits an object graph, where next objects to visit are calculated by
    /// invoking the specified <paramref name="nodeRangeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/> collection.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="nodeRangeSelector">Descendant node range selector.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// </returns>
    /// <remarks>Objects are traversed in breadth-first order.</remarks>
    [Pure]
    public static IEnumerable<T> VisitMany<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> nodeRangeSelector)
    {
        var nodesToVisit = new Queue<T>( source );

        while ( nodesToVisit.Count > 0 )
        {
            var parent = nodesToVisit.Dequeue();
            var nodes = nodeRangeSelector( parent );

            foreach ( var n in nodes )
                nodesToVisit.Enqueue( n );

            yield return parent;
        }
    }

    /// <summary>
    /// Recursively visits an object graph, where next objects to visit are calculated by
    /// invoking the specified <paramref name="nodeRangeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/> collection.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="nodeRangeSelector">Descendant node range selector.</param>
    /// <param name="stopPredicate">Predicate that stops the traversal for the given sub-graph, when it returns <b>true</b>.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// </returns>
    /// <remarks>Objects are traversed in breadth-first order.</remarks>
    [Pure]
    public static IEnumerable<T> VisitMany<T>(
        this IEnumerable<T> source,
        Func<T, IEnumerable<T>> nodeRangeSelector,
        Func<T, bool> stopPredicate)
    {
        var nodesToVisit = new Queue<T>( source );

        while ( nodesToVisit.Count > 0 )
        {
            var parent = nodesToVisit.Dequeue();

            if ( stopPredicate( parent ) )
            {
                yield return parent;

                continue;
            }

            var nodes = nodeRangeSelector( parent );

            foreach ( var n in nodes )
                nodesToVisit.Enqueue( n );

            yield return parent;
        }
    }

    /// <summary>
    /// Attempts to compute an aggregation for the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="func">Aggregator delegate.</param>
    /// <param name="result"><b>out</b> parameter that contains aggregation result, when <paramref name="source"/> is not empty.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    public static bool TryAggregate<T>(this IEnumerable<T> source, Func<T, T, T> func, [MaybeNullWhen( false )] out T result)
    {
        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
        {
            result = default;
            return false;
        }

        result = enumerator.Current;

        while ( enumerator.MoveNext() )
            result = func( result, enumerator.Current );

        return true;
    }

    /// <summary>
    /// Finds elements with minimum and maximum values specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/> by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>A tuple containing elements with <b>Min</b> and <b>Max</b> values.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="source"/> is empty.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max) MinMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
    {
        return source.MinMaxBy( selector, selector );
    }

    /// <summary>
    /// Finds elements with minimum and maximum values specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>A tuple containing elements with <b>Min</b> and <b>Max</b> values.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="source"/> is empty.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max) MinMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
    {
        return source.MinMaxBy( selector, selector, comparer, comparer );
    }

    /// <summary>
    /// Finds elements with minimum and maximum values specified by <paramref name="minSelector"/> and <paramref name="maxSelector"/>
    /// in the provided <paramref name="source"/> by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="minSelector">Selector of a minimum value to use for comparison.</param>
    /// <param name="maxSelector">Selector of a maximum value to use for comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Type used for minimum value comparison.</typeparam>
    /// <typeparam name="T3">Type used for maximum value comparison.</typeparam>
    /// <returns>A tuple containing elements with <b>Min</b> and <b>Max</b> values.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="source"/> is empty.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max) MinMaxBy<T1, T2, T3>(
        this IEnumerable<T1> source,
        Func<T1, T2> minSelector,
        Func<T1, T3> maxSelector)
    {
        return source.MinMaxBy( minSelector, maxSelector, Comparer<T2>.Default, Comparer<T3>.Default );
    }

    /// <summary>
    /// Finds elements with minimum and maximum values specified by <paramref name="minSelector"/> and <paramref name="maxSelector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="minSelector">Selector of a minimum value to use for comparison.</param>
    /// <param name="maxSelector">Selector of a maximum value to use for comparison.</param>
    /// <param name="minComparer">Comparer to use for minimum value comparison.</param>
    /// <param name="maxComparer">Comparer to use for maximum value comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Type used for minimum value comparison.</typeparam>
    /// <typeparam name="T3">Type used for maximum value comparison.</typeparam>
    /// <returns>A tuple containing elements with <b>Min</b> and <b>Max</b> values.</returns>
    /// <exception cref="InvalidOperationException">When <paramref name="source"/> is empty.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max) MinMaxBy<T1, T2, T3>(
        this IEnumerable<T1> source,
        Func<T1, T2> minSelector,
        Func<T1, T3> maxSelector,
        IComparer<T2> minComparer,
        IComparer<T3> maxComparer)
    {
        var result = source.TryMinMaxBy( minSelector, maxSelector, minComparer, maxComparer );
        if ( result is null )
            ExceptionThrower.Throw( new InvalidOperationException( ExceptionResources.SequenceContainsNoElements ) );

        return result.Value;
    }

    /// <summary>
    /// Attempts to find an element with the maximum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/> using the <see cref="Comparer{T2}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="result"><b>out</b> parameter that contains the element with maximum value, if the collection is not empty.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, [MaybeNullWhen( false )] out T1 result)
    {
        return source.TryMaxBy( selector, Comparer<T2>.Default, out result );
    }

    /// <summary>
    /// Attempts to find an element with the maximum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <param name="result"><b>out</b> parameter that contains the element with maximum value, if the collection is not empty.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMaxBy<T1, T2>(
        this IEnumerable<T1> source,
        Func<T1, T2> selector,
        IComparer<T2> comparer,
        [MaybeNullWhen( false )] out T1 result)
    {
        return source.TryAggregate( (a, b) => comparer.Compare( selector( a ), selector( b ) ) > 0 ? a : b, out result );
    }

    /// <summary>
    /// Attempts to find an element with the minimum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/> using the <see cref="Comparer{T2}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="result"><b>out</b> parameter that contains the element with minimum value, if the collection is not empty.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMinBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, [MaybeNullWhen( false )] out T1 result)
    {
        return source.TryMinBy( selector, Comparer<T2>.Default, out result );
    }

    /// <summary>
    /// Attempts to find an element with the minimum value specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <param name="result"><b>out</b> parameter that contains the element with minimum value, if the collection is not empty.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is not empty, otherwise <b>false</b>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool TryMinBy<T1, T2>(
        this IEnumerable<T1> source,
        Func<T1, T2> selector,
        IComparer<T2> comparer,
        [MaybeNullWhen( false )] out T1 result)
    {
        return source.TryAggregate( (a, b) => comparer.Compare( selector( a ), selector( b ) ) < 0 ? a : b, out result );
    }

    /// <summary>
    /// Attempts to find elements with minimum and maximum values specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/> using the <see cref="Comparer{T2}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>
    /// A tuple containing elements with <b>Min</b> and <b>Max</b> values, or null when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max)? TryMinMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector)
    {
        return source.TryMinMaxBy( selector, Comparer<T2>.Default );
    }

    /// <summary>
    /// Attempts to find elements with minimum and maximum values specified by the <paramref name="selector"/>
    /// in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="selector">Selector of a value to use for comparison.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Value type used for comparison.</typeparam>
    /// <returns>
    /// A tuple containing elements with <b>Min</b> and <b>Max</b> values, or null when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max)? TryMinMaxBy<T1, T2>(this IEnumerable<T1> source, Func<T1, T2> selector, IComparer<T2> comparer)
    {
        return source.TryMinMaxBy( selector, selector, comparer, comparer );
    }

    /// <summary>
    /// Attempts to find elements with minimum and maximum values specified by <paramref name="minSelector"/>
    /// and <paramref name="maxSelector"/> in the provided <paramref name="source"/> using the <see cref="Comparer{T2}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="minSelector">Selector of a minimum value to use for comparison.</param>
    /// <param name="maxSelector">Selector of a maximum value to use for comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Type used for minimum value comparison.</typeparam>
    /// <typeparam name="T3">Type used for maximum value comparison.</typeparam>
    /// <returns>
    /// A tuple containing elements with <b>Min</b> and <b>Max</b> values, or null when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T1 Min, T1 Max)? TryMinMaxBy<T1, T2, T3>(
        this IEnumerable<T1> source,
        Func<T1, T2> minSelector,
        Func<T1, T3> maxSelector)
    {
        return source.TryMinMaxBy( minSelector, maxSelector, Comparer<T2>.Default, Comparer<T3>.Default );
    }

    /// <summary>
    /// Attempts to find elements with minimum and maximum values specified by <paramref name="minSelector"/>
    /// and <paramref name="maxSelector"/> in the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="minSelector">Selector of a minimum value to use for comparison.</param>
    /// <param name="maxSelector">Selector of a maximum value to use for comparison.</param>
    /// <param name="minComparer">Comparer to use for minimum value comparison.</param>
    /// <param name="maxComparer">Comparer to use for maximum value comparison.</param>
    /// <typeparam name="T1">Collection element type.</typeparam>
    /// <typeparam name="T2">Type used for minimum value comparison.</typeparam>
    /// <typeparam name="T3">Type used for maximum value comparison.</typeparam>
    /// <returns>
    /// A tuple containing elements with <b>Min</b> and <b>Max</b> values, or null when <paramref name="source"/> is empty.
    /// </returns>
    [Pure]
    public static (T1 Min, T1 Max)? TryMinMaxBy<T1, T2, T3>(
        this IEnumerable<T1> source,
        Func<T1, T2> minSelector,
        Func<T1, T3> maxSelector,
        IComparer<T2> minComparer,
        IComparer<T3> maxComparer)
    {
        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return null;

        var min = enumerator.Current;
        var max = min;
        var minValue = minSelector( min );
        var maxValue = maxSelector( max );

        while ( enumerator.MoveNext() )
        {
            var current = enumerator.Current;
            var currentMinValue = minSelector( current );
            var currentMaxValue = maxSelector( current );

            if ( minComparer.Compare( minValue, currentMinValue ) > 0 )
            {
                min = current;
                minValue = currentMinValue;
            }

            if ( maxComparer.Compare( maxValue, currentMaxValue ) < 0 )
            {
                max = current;
                maxValue = currentMaxValue;
            }
        }

        return (min, max);
    }

    /// <summary>
    /// Return a new <see cref="IEnumerable{T}"/> that contains the result of performing left outer join on two collections
    /// by using the <see cref="EqualityComparer{TKey}.Default"/> key equality comparer.
    /// </summary>
    /// <param name="outer">Outer collection.</param>
    /// <param name="inner">Inner collection.</param>
    /// <param name="outerKeySelector">Selector of outer collection element keys.</param>
    /// <param name="innerKeySelector">Selector of inner collection element keys.</param>
    /// <param name="resultSelector">Joined elements result selector.</param>
    /// <typeparam name="T1">Outer collection element type.</typeparam>
    /// <typeparam name="T2">Inner collection element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TResult> LeftJoin<T1, T2, TKey, TResult>(
        this IEnumerable<T1> outer,
        IEnumerable<T2> inner,
        Func<T1, TKey> outerKeySelector,
        Func<T2, TKey> innerKeySelector,
        Func<T1, T2?, TResult> resultSelector)
    {
        return outer.LeftJoin( inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Return a new <see cref="IEnumerable{T}"/> that contains the result of performing left outer join on two collections.
    /// </summary>
    /// <param name="outer">Outer collection.</param>
    /// <param name="inner">Inner collection.</param>
    /// <param name="outerKeySelector">Selector of outer collection element keys.</param>
    /// <param name="innerKeySelector">Selector of inner collection element keys.</param>
    /// <param name="resultSelector">Joined elements result selector.</param>
    /// <param name="keyComparer">Comparer to use for key equality comparison.</param>
    /// <typeparam name="T1">Outer collection element type.</typeparam>
    /// <typeparam name="T2">Inner collection element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<TResult> LeftJoin<T1, T2, TKey, TResult>(
        this IEnumerable<T1> outer,
        IEnumerable<T2> inner,
        Func<T1, TKey> outerKeySelector,
        Func<T2, TKey> innerKeySelector,
        Func<T1, T2?, TResult> resultSelector,
        IEqualityComparer<TKey> keyComparer)
    {
        var groups = outer
            .GroupJoin(
                inner,
                outerKeySelector,
                innerKeySelector,
                static (o, i) => (Outer: o, Inner: i),
                keyComparer );

        foreach ( var (outerElement, innerElementRange) in groups )
        {
            using var innerGroupEnumerator = innerElementRange.GetEnumerator();

            if ( ! innerGroupEnumerator.MoveNext() )
            {
                yield return resultSelector( outerElement, default );

                continue;
            }

            yield return resultSelector( outerElement, innerGroupEnumerator.Current );

            while ( innerGroupEnumerator.MoveNext() )
                yield return resultSelector( outerElement, innerGroupEnumerator.Current );
        }
    }

    /// <summary>
    /// Return a new <see cref="IEnumerable{T}"/> that contains the result of performing full outer join on two collections
    /// by using the <see cref="EqualityComparer{TKey}.Default"/> key equality comparer.
    /// </summary>
    /// <param name="outer">Outer collection.</param>
    /// <param name="inner">Inner collection.</param>
    /// <param name="outerKeySelector">Selector of outer collection element keys.</param>
    /// <param name="innerKeySelector">Selector of inner collection element keys.</param>
    /// <param name="resultSelector">Joined elements result selector.</param>
    /// <typeparam name="T1">Outer collection element type.</typeparam>
    /// <typeparam name="T2">Inner collection element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<TResult> FullJoin<T1, T2, TKey, TResult>(
        this IEnumerable<T1> outer,
        IEnumerable<T2> inner,
        Func<T1, TKey> outerKeySelector,
        Func<T2, TKey> innerKeySelector,
        Func<T1?, T2?, TResult> resultSelector)
    {
        return outer.FullJoin( inner, outerKeySelector, innerKeySelector, resultSelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Return a new <see cref="IEnumerable{T}"/> that contains the result of performing full outer join on two collections.
    /// </summary>
    /// <param name="outer">Outer collection.</param>
    /// <param name="inner">Inner collection.</param>
    /// <param name="outerKeySelector">Selector of outer collection element keys.</param>
    /// <param name="innerKeySelector">Selector of inner collection element keys.</param>
    /// <param name="resultSelector">Joined elements result selector.</param>
    /// <param name="keyComparer">Comparer to use for key equality comparison.</param>
    /// <typeparam name="T1">Outer collection element type.</typeparam>
    /// <typeparam name="T2">Inner collection element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TResult">Result type.</typeparam>
    /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
    [Pure]
    public static IEnumerable<TResult> FullJoin<T1, T2, TKey, TResult>(
        this IEnumerable<T1> outer,
        IEnumerable<T2> inner,
        Func<T1, TKey> outerKeySelector,
        Func<T2, TKey> innerKeySelector,
        Func<T1?, T2?, TResult> resultSelector,
        IEqualityComparer<TKey> keyComparer)
    {
        var innerMap = inner.ToLookup( innerKeySelector, keyComparer );
        var joinedOuterKeys = new HashSet<TKey>( keyComparer );

        foreach ( var o in outer )
        {
            var key = outerKeySelector( o );
            var innerGroup = innerMap[key];

            using var innerGroupEnumerator = innerGroup.GetEnumerator();

            if ( ! innerGroupEnumerator.MoveNext() )
            {
                yield return resultSelector( o, default );

                continue;
            }

            yield return resultSelector( o, innerGroupEnumerator.Current );

            while ( innerGroupEnumerator.MoveNext() )
                yield return resultSelector( o, innerGroupEnumerator.Current );

            joinedOuterKeys.Add( key );
        }

        foreach ( var g in innerMap )
        {
            if ( joinedOuterKeys.Contains( g.Key ) )
                continue;

            foreach ( var i in g )
                yield return resultSelector( default, i );
        }
    }

    /// <summary>
    /// Creates a new <see cref="IEnumerable{T}"/> instance that represents a slice, or a sub-range, of <paramref name="source"/> elements.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="startIndex">Index of the first element to include in the slice.</param>
    /// <param name="length">Length of the slice.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance,
    /// or an empty enumerable when <paramref name="length"/> is less than <b>1</b>
    /// or when computed index of the last slice element is less than <b>1</b>.
    /// </returns>
    [Pure]
    public static IEnumerable<T> Slice<T>(this IEnumerable<T> source, int startIndex, int length)
    {
        if ( length <= 0 )
            return Enumerable.Empty<T>();

        if ( startIndex < 0 )
        {
            length += startIndex;
            if ( length <= 0 )
                return Enumerable.Empty<T>();

            startIndex = 0;
        }

        return SliceIterator( source, startIndex, length );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> elements are ordered
    /// by using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is ordered, otherwise <b>false</b>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static bool IsOrdered<T>(this IEnumerable<T> source)
    {
        return source.IsOrdered( Comparer<T>.Default );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="source"/> elements are ordered.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Comparer to use for value comparison.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns><b>true</b> when <paramref name="source"/> is ordered, otherwise <b>false</b>.</returns>
    [Pure]
    public static bool IsOrdered<T>(this IEnumerable<T> source, IComparer<T> comparer)
    {
        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            return true;

        var previous = enumerator.Current;

        while ( enumerator.MoveNext() )
        {
            var next = enumerator.Current;
            if ( comparer.Compare( previous, next ) > 0 )
                return false;

            previous = next;
        }

        return true;
    }

    /// <summary>
    /// Partitions the provided <paramref name="source"/> into two groups:
    /// elements that pass the specified <paramref name="predicate"/> and elements that fail.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="predicate">Predicate to use for collection partitioning.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>New <see cref="PartitionResult{T}"/> instance.</returns>
    /// <remarks>Partitioning creates a new materialized collection.</remarks>
    [Pure]
    public static PartitionResult<T> Partition<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        using var enumerator = source.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return default;

        var passedCount = 0;
        var items = source.TryGetNonEnumeratedCount( out var count ) ? new List<T>( capacity: count ) : new List<T>();

        do
        {
            var current = enumerator.Current;
            items.Add( current );

            if ( ! predicate( current ) )
                continue;

            (items[^1], items[passedCount]) = (items[passedCount], items[^1]);
            ++passedCount;
        }
        while ( enumerator.MoveNext() );

        return new PartitionResult<T>( items, passedCount );
    }

    /// <summary>
    /// Converts provided <paramref name="source"/> to <see cref="ReadOnlyMemory{T}"/> instance.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Collection element type.</typeparam>
    /// <returns>New <see cref="ReadOnlyMemory{T}"/> instance.</returns>
    /// <remarks>
    /// New array will not be allocated when the provided <paramref name="source"/> itself is an array
    /// or a string or an enumerable whose count can be cheaply extracted and it is empty.
    /// </remarks>
    [Pure]
    public static ReadOnlyMemory<T> ToMemory<T>(this IEnumerable<T> source)
    {
        if ( typeof( T ) == typeof( char ) )
        {
            if ( source is string str )
                return ( ReadOnlyMemory<T> )( object )str.AsMemory();
        }

        if ( source is T[] arr )
            return arr.AsMemory();

        if ( source.TryGetNonEnumeratedCount( out var count ) && count == 0 )
            return ReadOnlyMemory<T>.Empty;

        return source.ToArray().AsMemory();
    }

    [Pure]
    private static IReadOnlySet<T> GetSet<T>(IEnumerable<T> source, IEqualityComparer<T> comparer)
    {
        if ( source is HashSet<T> hashSet && hashSet.Comparer.Equals( comparer ) )
            return hashSet;

        return new HashSet<T>( source, comparer );
    }

    [Pure]
    private static bool SetEquals<T>(IReadOnlySet<T> set, IReadOnlySet<T> other)
    {
        if ( set.Count != other.Count )
            return false;

        foreach ( var o in other )
        {
            if ( ! set.Contains( o ) )
                return false;
        }

        return true;
    }

    [Pure]
    private static IEnumerable<T> SliceIterator<T>(IEnumerable<T> source, int startIndex, int length)
    {
        Assume.IsGreaterThanOrEqualTo( startIndex, 0 );
        Assume.IsGreaterThanOrEqualTo( length, 1 );

        using var enumerator = source.GetEnumerator();

        if ( ! enumerator.MoveNext() )
            yield break;

        var skipped = 0;
        while ( skipped < startIndex && enumerator.MoveNext() )
            ++skipped;

        if ( skipped != startIndex )
            yield break;

        yield return enumerator.Current;

        var taken = 1;
        while ( taken < length && enumerator.MoveNext() )
        {
            ++taken;
            yield return enumerator.Current;
        }
    }
}
