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
using System.Linq;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only collection of (key, value-range) pairs.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public interface IReadOnlyMultiDictionary<TKey, TValue> : IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>, ILookup<TKey, TValue>
    where TKey : notnull
{
    /// <inheritdoc cref="IReadOnlyCollection{T}.Count" />
    new int Count { get; }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.this" />
    new IReadOnlyList<TValue> this[TKey key] { get; }

    /// <summary>
    /// Key equality comparer.
    /// </summary>
    IEqualityComparer<TKey> Comparer { get; }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Keys" />
    new IReadOnlyCollection<TKey> Keys { get; }

    /// <inheritdoc cref="IReadOnlyDictionary{TKey,TValue}.Values" />
    new IReadOnlyCollection<IReadOnlyList<TValue>> Values { get; }

    /// <summary>
    /// Returns the number of elements associated with the provided <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Number of elements associated with the provided <paramref name="key"/>.</returns>
    [Pure]
    int GetCount(TKey key);

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()" />
    [Pure]
    new IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetEnumerator();
}
