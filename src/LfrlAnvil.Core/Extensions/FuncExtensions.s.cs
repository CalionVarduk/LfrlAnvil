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
/// Contains <see cref="Func{TResult}"/> extension methods.
/// </summary>
public static class FuncExtensions
{
    /// <summary>
    /// Creates a new <see cref="Lazy{T}"/> instance based on the provided <paramref name="source"/> delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Lazy{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Lazy<T> ToLazy<T>(this Func<T> source)
    {
        return new Lazy<T>( source );
    }

    /// <summary>
    /// Creates a new <see cref="IMemoizedCollection{T}"/> instance based on the provided <paramref name="source"/> delegate.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>
    /// New <see cref="IMemoizedCollection{T}"/> instance that lazily materializes the result of the provided <paramref name="source"/>.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IMemoizedCollection<T> Memoize<T>(this Func<IEnumerable<T>> source)
    {
        return source().Memoize();
    }

    /// <summary>
    /// Creates a new <see cref="Action"/> delegate based on the provided <paramref name="source"/>,
    /// that invokes the source and discards its result.
    /// </summary>
    /// <param name="source">Source delegate.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="Action"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Action IgnoreResult<T>(this Func<T> source)
    {
        return () => source();
    }
}
