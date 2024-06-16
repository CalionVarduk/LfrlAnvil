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

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains various object extension methods.
/// </summary>
public static class ObjectExtensions
{
    /// <summary>
    /// Creates a new <see cref="Ref{T}"/> instance from the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="Ref{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Ref<T> ToRef<T>(this T source)
    {
        return Ref.Create( source );
    }

    /// <summary>
    /// Creates a new <see cref="Nullable{T}"/> instance from the given non-null <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>New <see cref="Nullable{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T? ToNullable<T>(this T source)
        where T : struct
    {
        return source;
    }

    /// <summary>
    /// Creates a new <see cref="IMemoizedCollection{T}"/> instance from the result of <paramref name="selector"/> invocation
    /// with <paramref name="source"/> as its parameter.
    /// </summary>
    /// <param name="source">Source object.</param>
    /// <param name="selector">Selector to invoke with <paramref name="source"/> as its parameter.</param>
    /// <typeparam name="T1">Source object type.</typeparam>
    /// <typeparam name="T2">Collection element type.</typeparam>
    /// <returns>New <see cref="IMemoizedCollection{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IMemoizedCollection<T2> Memoize<T1, T2>(this T1 source, Func<T1, IEnumerable<T2>> selector)
    {
        return selector( source ).Memoize();
    }

    /// <summary>
    /// Recursively visits a chain of objects, where next object in the chain is calculated by
    /// invoking the specified <paramref name="nodeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// Traversal ends when <paramref name="nodeSelector"/> returns null, or when <paramref name="source"/> is null.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeSelector">Descendant node selector.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is not included.
    /// </returns>
    /// <seealso cref="ObjectExtensions.VisitWithSelf{T}(T,Func{T,T})"/>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> Visit<T>(this T? source, Func<T, T?> nodeSelector)
        where T : class
    {
        return source.Visit( nodeSelector!, static e => e is null )!;
    }

    /// <summary>
    /// Recursively visits a chain of objects, where next object in the chain is calculated by
    /// invoking the specified <paramref name="nodeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// Traversal ends when the <paramref name="breakPredicate"/> returns <b>true</b>.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeSelector">Descendant node selector.</param>
    /// <param name="breakPredicate">Predicate that ends the traversal when it returns <b>true</b>.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is not included.
    /// </returns>
    /// <seealso cref="ObjectExtensions.VisitWithSelf{T}(T,Func{T,T},Func{T,Boolean})"/>
    [Pure]
    public static IEnumerable<T> Visit<T>(this T source, Func<T, T> nodeSelector, Func<T, bool> breakPredicate)
    {
        if ( breakPredicate( source ) )
            yield break;

        var current = nodeSelector( source );

        while ( ! breakPredicate( current ) )
        {
            yield return current;

            current = nodeSelector( current );
        }
    }

    /// <summary>
    /// Recursively visits an object graph, where next objects to visit are calculated by
    /// invoking the specified <paramref name="nodeRangeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeRangeSelector">Descendant node range selector.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is not included.
    /// </returns>
    /// <seealso cref="ObjectExtensions.VisitManyWithSelf{T}(T,Func{T,IEnumerable{T}})"/>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> VisitMany<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
    {
        return nodeRangeSelector( source ).VisitMany( nodeRangeSelector );
    }

    /// <summary>
    /// Recursively visits an object graph, where next objects to visit are calculated by
    /// invoking the specified <paramref name="nodeRangeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeRangeSelector">Descendant node range selector.</param>
    /// <param name="stopPredicate">Predicate that stops the traversal for the given sub-graph, when it returns <b>true</b>.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is not included.
    /// </returns>
    /// <seealso cref="ObjectExtensions.VisitManyWithSelf{T}(T,Func{T,IEnumerable{T}},Func{T,Boolean})"/>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> VisitMany<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector, Func<T, bool> stopPredicate)
    {
        if ( stopPredicate( source ) )
            return Enumerable.Empty<T>();

        return nodeRangeSelector( source ).VisitMany( nodeRangeSelector, stopPredicate );
    }

    /// <summary>
    /// Recursively visits a chain of objects, where next object in the chain is calculated by
    /// invoking the specified <paramref name="nodeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// Traversal ends when <paramref name="nodeSelector"/> returns null, or when <paramref name="source"/> is null.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeSelector">Descendant node selector.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is included.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> VisitWithSelf<T>(this T? source, Func<T, T?> nodeSelector)
        where T : class
    {
        return source.VisitWithSelf( nodeSelector!, static e => e is null )!;
    }

    /// <summary>
    /// Recursively visits a chain of objects, where next object in the chain is calculated by
    /// invoking the specified <paramref name="nodeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// Traversal ends when the <paramref name="breakPredicate"/> returns <b>true</b>.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeSelector">Descendant node selector.</param>
    /// <param name="breakPredicate">Predicate that ends the traversal when it returns <b>true</b>.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is included.
    /// </returns>
    [Pure]
    public static IEnumerable<T> VisitWithSelf<T>(this T source, Func<T, T> nodeSelector, Func<T, bool> breakPredicate)
    {
        if ( breakPredicate( source ) )
            yield break;

        yield return source;

        var current = nodeSelector( source );

        while ( ! breakPredicate( current ) )
        {
            yield return current;

            current = nodeSelector( current );
        }
    }

    /// <summary>
    /// Recursively visits an object graph, where next objects to visit are calculated by
    /// invoking the specified <paramref name="nodeRangeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeRangeSelector">Descendant node range selector.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is included.
    /// </returns>
    /// <remarks>See <see cref="EnumerableExtensions.VisitMany{T}(IEnumerable{T},Func{T,IEnumerable{T}})"/> for more information.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> VisitManyWithSelf<T>(this T source, Func<T, IEnumerable<T>> nodeRangeSelector)
    {
        return source.VisitMany( nodeRangeSelector ).Prepend( source );
    }

    /// <summary>
    /// Recursively visits an object graph, where next objects to visit are calculated by
    /// invoking the specified <paramref name="nodeRangeSelector"/> with current object as its parameter,
    /// starting with the given <paramref name="source"/>.
    /// </summary>
    /// <param name="source">Source object to start from.</param>
    /// <param name="nodeRangeSelector">Descendant node range selector.</param>
    /// <param name="stopPredicate">Predicate that stops the traversal for the given sub-graph, when it returns <b>true</b>.</param>
    /// <typeparam name="T">Object type.</typeparam>
    /// <returns>
    /// New <see cref="IEnumerable{T}"/> instance that contains all recursively visited objects, in order of traversal.
    /// <paramref name="source"/> object is included.
    /// </returns>
    /// <remarks>
    /// See <see cref="EnumerableExtensions.VisitMany{T}(IEnumerable{T},Func{T,IEnumerable{T}},Func{T,Boolean})"/> for more information.
    /// </remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IEnumerable<T> VisitManyWithSelf<T>(
        this T source,
        Func<T, IEnumerable<T>> nodeRangeSelector,
        Func<T, bool> stopPredicate)
    {
        return source.VisitMany( nodeRangeSelector, stopPredicate ).Prepend( source );
    }

    /// <summary>
    /// Returns the lesser value out of the two.
    /// </summary>
    /// <param name="source">First value.</param>
    /// <param name="other">Second value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <paramref name="source"/> when it is less than or equal to <paramref name="other"/>, otherwise <paramref name="other"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T Min<T>(this T source, T other)
        where T : IComparable<T>
    {
        return source.CompareTo( other ) <= 0 ? source : other;
    }

    /// <summary>
    /// Returns the greater value out of the two.
    /// </summary>
    /// <param name="source">First value.</param>
    /// <param name="other">Second value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <paramref name="source"/> when it is greater than <paramref name="other"/>, otherwise <paramref name="other"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static T Max<T>(this T source, T other)
        where T : IComparable<T>
    {
        return source.CompareTo( other ) <= 0 ? other : source;
    }

    /// <summary>
    /// Splits the two provided values into lesser and greater.
    /// </summary>
    /// <param name="source">First value.</param>
    /// <param name="other">Second value.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// <paramref name="source"/> as <b>Min</b> and <paramref name="other"/> as <b>Max</b>
    /// when <paramref name="source"/> is less than or equal to <paramref name="other"/>,
    /// otherwise <paramref name="other"/> as <b>Min</b> and <paramref name="source"/> as <b>Max</b>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static (T Min, T Max) MinMax<T>(this T source, T other)
        where T : IComparable<T>
    {
        return source.CompareTo( other ) <= 0 ? (source, other) : (other, source);
    }
}
