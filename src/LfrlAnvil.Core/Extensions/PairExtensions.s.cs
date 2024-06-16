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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="Pair{T1,T2}"/> related extension methods.
/// </summary>
public static class PairExtensions
{
    /// <summary>
    /// Creates a new <see cref="Pair{T1,T2}"/> instance from the given tuple.
    /// </summary>
    /// <param name="source">Source tuple.</param>
    /// <typeparam name="T1">Pair's first item type.</typeparam>
    /// <typeparam name="T2">Pair's second item type.</typeparam>
    /// <returns>New <see cref="Pair{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Pair<T1, T2> ToPair<T1, T2>(this Tuple<T1, T2> source)
    {
        return Pair.Create( source.Item1, source.Item2 );
    }

    /// <summary>
    /// Creates a new <see cref="Pair{T1,T2}"/> instance from the given tuple.
    /// </summary>
    /// <param name="source">Source tuple.</param>
    /// <typeparam name="T1">Pair's first item type.</typeparam>
    /// <typeparam name="T2">Pair's second item type.</typeparam>
    /// <returns>New <see cref="Pair{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Pair<T1, T2> ToPair<T1, T2>(this ValueTuple<T1, T2> source)
    {
        return Pair.Create( source.Item1, source.Item2 );
    }

    /// <summary>
    /// Creates a new <see cref="Tuple{T1,T2}"/> instance from the given pair.
    /// </summary>
    /// <param name="source">Source pair.</param>
    /// <typeparam name="T1">Pair's first item type.</typeparam>
    /// <typeparam name="T2">Pair's second item type.</typeparam>
    /// <returns>New <see cref="Tuple{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Tuple<T1, T2> ToTuple<T1, T2>(this Pair<T1, T2> source)
    {
        return Tuple.Create( source.First, source.Second );
    }

    /// <summary>
    /// Creates a new <see cref="ValueTuple{T1,T2}"/> instance from the given pair.
    /// </summary>
    /// <param name="source">Source pair.</param>
    /// <typeparam name="T1">Pair's first item type.</typeparam>
    /// <typeparam name="T2">Pair's second item type.</typeparam>
    /// <returns>New <see cref="ValueTuple{T1,T2}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTuple<T1, T2> ToValueTuple<T1, T2>(this Pair<T1, T2> source)
    {
        return ValueTuple.Create( source.First, source.Second );
    }

    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance created from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Conversion source.</param>
    /// <typeparam name="T">Pair value type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance with two elements:
    /// <see cref="Pair{T,T}.First"/> followed by <see cref="Pair{T,T}.Second"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T, T> source)
    {
        yield return source.First;
        yield return source.Second;
    }

    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance created from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Conversion source.</param>
    /// <typeparam name="T">Pair value type.</typeparam>
    /// <returns>
    /// If <see cref="Pair{T,T}.Second"/> is not null, then a new <see cref="IEnumerable{T}"/> instance with two elements:
    /// <see cref="Pair{T,T}.First"/> followed by <see cref="Pair{T,T}.Second"/>.
    /// Otherwise, a new <see cref="IEnumerable{T}"/> instance with a single <see cref="Pair{T,T}.First"/> element.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T, T?> source)
        where T : struct
    {
        yield return source.First;

        if ( source.Second.HasValue )
            yield return source.Second.Value;
    }

    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance created from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Conversion source.</param>
    /// <typeparam name="T">Pair value type.</typeparam>
    /// <returns>
    /// If <see cref="Pair{T,T}.First"/> is not null, then a new <see cref="IEnumerable{T}"/> instance with two elements:
    /// <see cref="Pair{T,T}.First"/> followed by <see cref="Pair{T,T}.Second"/>.
    /// Otherwise, a new <see cref="IEnumerable{T}"/> instance with a single <see cref="Pair{T,T}.Second"/> element.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> AsEnumerable<T>(this Pair<T?, T> source)
        where T : struct
    {
        if ( source.First.HasValue )
            yield return source.First.Value;

        yield return source.Second;
    }

    /// <summary>
    /// Returns a new <see cref="IEnumerable{T}"/> instance created from the provided <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Conversion source.</param>
    /// <typeparam name="T">Pair value type.</typeparam>
    /// <returns>
    /// If <see cref="Pair{T,T}.First"/> is not null and <see cref="Pair{T,T}.Second"/> is not null,
    /// then a new <see cref="IEnumerable{T}"/> instance with two elements:
    /// <see cref="Pair{T,T}.First"/> followed by <see cref="Pair{T,T}.Second"/>.
    /// Otherwise, if <see cref="Pair{T,T}.First"/> is not null,
    /// then a new <see cref="IEnumerable{T}"/> instance with a single <see cref="Pair{T,T}.First"/> element.
    /// Otherwise, if <see cref="Pair{T,T}.Second"/> is not null,
    /// then a new <see cref="IEnumerable{T}"/> instance with a single <see cref="Pair{T,T}.Second"/> element.
    /// Otherwise, an empty <see cref="IEnumerable{T}"/> instance.
    /// </returns>
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

    /// <summary>
    /// Deconstruct the given <see cref="Pair{T1,T2}"/> instance.
    /// </summary>
    /// <param name="source">Source pair.</param>
    /// <param name="first"><b>out</b> parameter that returns <see cref="Pair{T1,T2}.First"/>.</param>
    /// <param name="second"><b>out</b> parameter that returns <see cref="Pair{T1,T2}.Second"/>.</param>
    /// <typeparam name="T1">Pair's first item type.</typeparam>
    /// <typeparam name="T2">Pair's second item type.</typeparam>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void Deconstruct<T1, T2>(this Pair<T1, T2> source, out T1 first, out T2 second)
    {
        first = source.First;
        second = source.Second;
    }
}
