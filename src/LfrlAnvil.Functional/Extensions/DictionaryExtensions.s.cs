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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Functional.Extensions;

/// <summary>
/// Contains dictionary extension methods.
/// </summary>
public static class DictionaryExtensions
{
    /// <summary>
    /// Attempts to get a value associated with the provided <paramref name="key"/> from the <paramref name="dictionary"/>.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to value associated with the provided <paramref name="key"/>
    /// or <see cref="Maybe{T}.None"/> when it does not exist.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
        where TValue : notnull
    {
        return dictionary.TryGetValue( key, out var result ) ? new Maybe<TValue>( result ) : Maybe<TValue>.None;
    }

    /// <summary>
    /// Attempts to get remove an entry associated with the provided <paramref name="key"/> from the <paramref name="dictionary"/>.
    /// </summary>
    /// <param name="dictionary">Source dictionary.</param>
    /// <param name="key">Entry's key.</param>
    /// <typeparam name="TKey">Key type.</typeparam>
    /// <typeparam name="TValue">Value type.</typeparam>
    /// <returns>
    /// New <see cref="Maybe{T}"/> instance equivalent to removed value associated with the provided <paramref name="key"/>
    /// or <see cref="Maybe{T}.None"/> when it does not exist.
    /// </returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static Maybe<TValue> TryRemove<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key)
        where TKey : notnull
        where TValue : notnull
    {
        return dictionary.Remove( key, out var removed ) ? new Maybe<TValue>( removed ) : Maybe<TValue>.None;
    }
}
