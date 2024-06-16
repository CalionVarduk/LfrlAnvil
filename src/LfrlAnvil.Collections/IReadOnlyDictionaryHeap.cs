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

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only heap data structure with the ability to identify entries by keys.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IReadOnlyDictionaryHeap<TKey, TValue> : IReadOnlyHeap<TValue>
{
    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> KeyComparer { get; }

    /// <summary>
    /// Returns the key associated with an entry located at the specified <paramref name="index"/>.
    /// </summary>
    /// <param name="index">0-based position.</param>
    /// <returns>Key associated with an entry located at the specified <paramref name="index"/>.</returns>
    /// <exception cref="IndexOutOfRangeException">
    /// When <paramref name="index"/> is not in [<b>0</b>, <see cref="IReadOnlyCollection{T}.Count"/>) range.
    /// </exception>
    [Pure]
    TKey GetKey(int index);

    /// <summary>
    /// Checks whether or not an entry with the specified <paramref name="key"/> exists in this heap.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns><b>true</b> when entry with the specified <paramref name="key"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool ContainsKey(TKey key);

    /// <summary>
    /// Returns an entry associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to get an entry for.</param>
    /// <returns>Entry associated with the specified <paramref name="key"/>.</returns>
    /// <exception cref="KeyNotFoundException">When <paramref name="key"/> does not exist in this heap.</exception>
    [Pure]
    TValue GetValue(TKey key);

    /// <summary>
    /// Attempts to return an entry associated with the specified <paramref name="key"/> if it exists.
    /// </summary>
    /// <param name="key">Key to get an entry for.</param>
    /// <param name="result"><b>out</b> parameter that returns an entry associated with the specified <paramref name="key"/>.</param>
    /// <returns><b>true</b> when key exists in this heap, otherwise <b>false</b>.</returns>
    bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result);
}
