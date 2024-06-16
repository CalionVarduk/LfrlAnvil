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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections.Extensions;

/// <summary>
/// Contains <see cref="IEnumerable{T}"/> extension methods.
/// </summary>
public static class EnumerableExtensions
{
    /// <summary>
    /// Creates a new <see cref="MultiHashSet{T}"/> instance from the provided collection
    /// with <see cref="EqualityComparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MultiHashSet{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiHashSet<T> ToMultiHashSet<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.ToMultiHashSet( EqualityComparer<T>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="MultiHashSet{T}"/> instance from the provided collection.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Element equality comparer.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="MultiHashSet{T}"/> instance.</returns>
    [Pure]
    public static MultiHashSet<T> ToMultiHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        where T : notnull
    {
        var result = new MultiHashSet<T>( comparer );
        foreach ( var e in source )
            result.Add( e );

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer, using the specified <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(
        this IEnumerable<TValue> source,
        Func<TValue, TKey> keySelector)
        where TKey : notnull
    {
        return source.ToMultiDictionary( keySelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection,
    /// using the specified <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="comparer">Key equality comparer.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer,
    /// using the specified <paramref name="keySelector"/> and <paramref name="valueSelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="valueSelector">Value selector.</param>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection,
    /// using the specified <paramref name="keySelector"/> and <paramref name="valueSelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="valueSelector">Value selector.</param>
    /// <param name="comparer">Key equality comparer.</param>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection of (key, value) pairs
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> source)
        where TKey : notnull
    {
        return source.ToMultiDictionary( EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection of (key, value) pairs.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Key equality comparer.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
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

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection of groups
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MultiDictionary<TKey, TValue> ToMultiDictionary<TKey, TValue>(this IEnumerable<IGrouping<TKey, TValue>> source)
        where TKey : notnull
    {
        return source.ToMultiDictionary( EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="MultiDictionary{TKey,TValue}"/> instance from the provided collection of groups.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Key equality comparer.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="MultiDictionary{TKey,TValue}"/> instance.</returns>
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

    /// <summary>
    /// Returns the provided <paramref name="source"/> as <see cref="IEnumerable{T}"/>.
    /// </summary>
    /// <param name="source">Source multi dictionary.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>Provided <paramref name="source"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<KeyValuePair<TKey, IReadOnlyList<TValue>>> AsEnumerable<TKey, TValue>(
        this IReadOnlyMultiDictionary<TKey, TValue> source)
        where TKey : notnull
    {
        return source;
    }

    /// <summary>
    /// Creates a new <see cref="SequentialHashSet{T}"/> instance from the provided collection
    /// with <see cref="EqualityComparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="SequentialHashSet{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SequentialHashSet<T> ToSequentialHashSet<T>(this IEnumerable<T> source)
        where T : notnull
    {
        return source.ToSequentialHashSet( EqualityComparer<T>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="SequentialHashSet{T}"/> instance from the provided collection.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="comparer">Element equality comparer.</param>
    /// <typeparam name="T">Element type.</typeparam>
    /// <returns>New <see cref="SequentialHashSet{T}"/> instance.</returns>
    [Pure]
    public static SequentialHashSet<T> ToSequentialHashSet<T>(this IEnumerable<T> source, IEqualityComparer<T> comparer)
        where T : notnull
    {
        var result = new SequentialHashSet<T>( comparer );
        foreach ( var e in source )
            result.Add( e );

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="SequentialDictionary{TKey,TValue}"/> instance from the provided collection
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer,
    /// using the specified <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <returns>New <see cref="SequentialDictionary{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When at least one key is not unique.</exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SequentialDictionary<TKey, TSource> ToSequentialDictionary<TSource, TKey>(
        this IEnumerable<TSource> source,
        Func<TSource, TKey> keySelector)
        where TKey : notnull
    {
        return source.ToSequentialDictionary( keySelector, EqualityComparer<TKey>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="SequentialDictionary{TKey,TValue}"/> instance from the provided collection,
    /// using the specified <paramref name="keySelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="comparer">Key equality comparer.</param>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <returns>New <see cref="SequentialDictionary{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When at least one key is not unique.</exception>
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

    /// <summary>
    /// Creates a new <see cref="SequentialDictionary{TKey,TValue}"/> instance from the provided collection
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer,
    /// using the specified <paramref name="keySelector"/> and <paramref name="valueSelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="valueSelector">Value selector.</param>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="SequentialDictionary{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When at least one key is not unique.</exception>
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

    /// <summary>
    /// Creates a new <see cref="SequentialDictionary{TKey,TValue}"/> instance from the provided collection,
    /// using the specified <paramref name="keySelector"/> and <paramref name="valueSelector"/>.
    /// </summary>
    /// <param name="source">Source collection.</param>
    /// <param name="keySelector">Key selector.</param>
    /// <param name="valueSelector">Value selector.</param>
    /// <param name="comparer">Key equality comparer.</param>
    /// <typeparam name="TSource">Source element type.</typeparam>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>New <see cref="SequentialDictionary{TKey,TValue}"/> instance.</returns>
    /// <exception cref="ArgumentException">When at least one key is not unique.</exception>
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
