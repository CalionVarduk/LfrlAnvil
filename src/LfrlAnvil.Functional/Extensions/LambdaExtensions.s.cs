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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Functional.Delegates;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains various delegate extensions.
/// </summary>
public static class LambdaExtensions
{
    /// <summary>
    /// Invokes the provided <paramref name="source"/> and encapsulates the result in an <see cref="Erratic{T}"/> instance
    /// with <see cref="Nil"/> value type.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<Nil> TryInvoke(this Action source)
    {
        return Erratic.Try( source );
    }

    /// <summary>
    /// Invokes the provided <paramref name="source"/> and encapsulates the result in an <see cref="Erratic{T}"/> instance.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>New <see cref="Erratic{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Erratic<T> TryInvoke<T>(this Func<T> source)
    {
        return Erratic.Try( source );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<Nil> ToFunc(this Action action)
    {
        return () =>
        {
            action();
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, Nil> ToFunc<T1>(this Action<T1> action)
    {
        return a1 =>
        {
            action( a1 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, Nil> ToFunc<T1, T2>(this Action<T1, T2> action)
    {
        return (a1, a2) =>
        {
            action( a1, a2 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, Nil> ToFunc<T1, T2, T3>(this Action<T1, T2, T3> action)
    {
        return (a1, a2, a3) =>
        {
            action( a1, a2, a3 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, Nil> ToFunc<T1, T2, T3, T4>(this Action<T1, T2, T3, T4> action)
    {
        return (a1, a2, a3, a4) =>
        {
            action( a1, a2, a3, a4 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, T5, Nil> ToFunc<T1, T2, T3, T4, T5>(this Action<T1, T2, T3, T4, T5> action)
    {
        return (a1, a2, a3, a4, a5) =>
        {
            action( a1, a2, a3, a4, a5 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, T5, T6, Nil> ToFunc<T1, T2, T3, T4, T5, T6>(this Action<T1, T2, T3, T4, T5, T6> action)
    {
        return (a1, a2, a3, a4, a5, a6) =>
        {
            action( a1, a2, a3, a4, a5, a6 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="action"/> and returns <see cref="Nil"/> value.
    /// </summary>
    /// <param name="action">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <typeparam name="T7">Delegate's seventh parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, T4, T5, T6, T7, Nil> ToFunc<T1, T2, T3, T4, T5, T6, T7>(
        this Action<T1, T2, T3, T4, T5, T6, T7> action)
    {
        return (a1, a2, a3, a4, a5, a6, a7) =>
        {
            action( a1, a2, a3, a4, a5, a6, a7 );
            return Nil.Instance;
        };
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action ToAction(this Func<Nil> func)
    {
        return () => func();
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1> ToAction<T1>(this Func<T1, Nil> func)
    {
        return a1 => func( a1 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2> ToAction<T1, T2>(this Func<T1, T2, Nil> func)
    {
        return (a1, a2) => func( a1, a2 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3> ToAction<T1, T2, T3>(this Func<T1, T2, T3, Nil> func)
    {
        return (a1, a2, a3) => func( a1, a2, a3 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4> ToAction<T1, T2, T3, T4>(this Func<T1, T2, T3, T4, Nil> func)
    {
        return (a1, a2, a3, a4) => func( a1, a2, a3, a4 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4, T5> ToAction<T1, T2, T3, T4, T5>(this Func<T1, T2, T3, T4, T5, Nil> func)
    {
        return (a1, a2, a3, a4, a5) => func( a1, a2, a3, a4, a5 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4, T5, T6> ToAction<T1, T2, T3, T4, T5, T6>(this Func<T1, T2, T3, T4, T5, T6, Nil> func)
    {
        return (a1, a2, a3, a4, a5, a6) => func( a1, a2, a3, a4, a5, a6 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/> and ignores its result.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's fourth parameter's type.</typeparam>
    /// <typeparam name="T5">Delegate's fifth parameter's type.</typeparam>
    /// <typeparam name="T6">Delegate's sixth parameter's type.</typeparam>
    /// <typeparam name="T7">Delegate's seventh parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action<T1, T2, T3, T4, T5, T6, T7> ToAction<T1, T2, T3, T4, T5, T6, T7>(
        this Func<T1, T2, T3, T4, T5, T6, T7, Nil> func)
    {
        return (a1, a2, a3, a4, a5, a6, a7) => func( a1, a2, a3, a4, a5, a6, a7 );
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/>
    /// and returns its <b>out</b> result as <see cref="Maybe{T}"/>.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's <b>out</b> parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<Maybe<T1>> Purify<T1>(this OutFunc<T1> func)
        where T1 : notnull
    {
        return () => func( out var result ) ? new Maybe<T1>( result ) : Maybe<T1>.None;
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/>
    /// and returns its <b>out</b> result as <see cref="Maybe{T}"/>.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's <b>out</b> parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, Maybe<T2>> Purify<T1, T2>(this OutFunc<T1, T2> func)
        where T2 : notnull
    {
        return t1 => func( t1, out var result ) ? new Maybe<T2>( result ) : Maybe<T2>.None;
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/>
    /// and returns its <b>out</b> result as <see cref="Maybe{T}"/>.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's <b>out</b> parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, Maybe<T3>> Purify<T1, T2, T3>(this OutFunc<T1, T2, T3> func)
        where T3 : notnull
    {
        return (t1, t2) => func( t1, t2, out var result ) ? new Maybe<T3>( result ) : Maybe<T3>.None;
    }

    /// <summary>
    /// Creates a new delegate that invokes the provided <paramref name="func"/>
    /// and returns its <b>out</b> result as <see cref="Maybe{T}"/>.
    /// </summary>
    /// <param name="func">Source delegate.</param>
    /// <typeparam name="T1">Delegate's first parameter's type.</typeparam>
    /// <typeparam name="T2">Delegate's second parameter's type.</typeparam>
    /// <typeparam name="T3">Delegate's third parameter's type.</typeparam>
    /// <typeparam name="T4">Delegate's <b>out</b> parameter's type.</typeparam>
    /// <returns>New delegate.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Func<T1, T2, T3, Maybe<T4>> Purify<T1, T2, T3, T4>(this OutFunc<T1, T2, T3, T4> func)
        where T4 : notnull
    {
        return (t1, t2, t3) => func( t1, t2, t3, out var result ) ? new Maybe<T4>( result ) : Maybe<T4>.None;
    }
}
