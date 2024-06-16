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

namespace LfrlAnvil;

/// <summary>
/// Creates instances of <see cref="IComparer{T}"/> type.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public static class ComparerFactory<T>
{
    /// <summary>
    /// Creates a new <see cref="IComparer{T}"/> instance that compares values
    /// by comparing results of the provided <paramref name="selector"/> using the <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="selector">Comparable value selector.</param>
    /// <typeparam name="TValue">Comparable value type.</typeparam>
    /// <returns>New <see cref="IComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> CreateBy<TValue>(Func<T?, TValue?> selector)
    {
        return CreateBy( selector, Comparer<TValue>.Default );
    }

    /// <summary>
    /// Creates a new <see cref="IComparer{T}"/> instance that compares values
    /// by comparing results of the provided <paramref name="selector"/>.
    /// </summary>
    /// <param name="selector">Comparable value selector.</param>
    /// <param name="valueComparer">Value comparer.</param>
    /// <typeparam name="TValue">Comparable value type.</typeparam>
    /// <returns>New <see cref="IComparer{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static IComparer<T> CreateBy<TValue>(Func<T?, TValue?> selector, IComparer<TValue> valueComparer)
    {
        return Comparer<T>.Create( (a, b) => valueComparer.Compare( selector( a ), selector( b ) ) );
    }
}
