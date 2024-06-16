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

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic multi set.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IMultiSet<T> : IReadOnlyMultiSet<T>, ISet<Pair<T, int>>
    where T : notnull
{
    /// <inheritdoc cref="ICollection{T}.Count" />
    new int Count { get; }

    /// <summary>
    /// Adds the provided <paramref name="item"/> to this set once.
    /// </summary>
    /// <param name="item">Element to add.</param>
    /// <returns>Current multiplicity of the added <paramref name="item"/>.</returns>
    int Add(T item);

    /// <summary>
    /// Adds the provided <paramref name="item"/> to this set with specified number of repetitions.
    /// </summary>
    /// <param name="item">Element to add.</param>
    /// <param name="count">Number of repetitions.</param>
    /// <returns>Current multiplicity of the added <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>1</b>.</exception>
    int AddMany(T item, int count);

    /// <summary>
    /// Removes the provided <paramref name="item"/> from this set once.
    /// </summary>
    /// <param name="item">Element to remove.</param>
    /// <returns>Current multiplicity of the removed <paramref name="item"/> or <b>-1</b> if it did not exist.</returns>
    int Remove(T item);

    /// <summary>
    /// Removes the provided <paramref name="item"/> from this set with specified number of repetitions.
    /// </summary>
    /// <param name="item">Element to remove.</param>
    /// <param name="count">Number of repetitions.</param>
    /// <returns>Current multiplicity of the removed <paramref name="item"/> or <b>-1</b> if it did not exist.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>1</b>.</exception>
    int RemoveMany(T item, int count);

    /// <summary>
    /// Removes all occurrences of the provided <paramref name="item"/> from this set.
    /// </summary>
    /// <param name="item">Element to remove.</param>
    /// <returns>Removed number of repetitions or <b>0</b> if <paramref name="item"/> did not exist.</returns>
    int RemoveAll(T item);

    /// <summary>
    /// Sets the number of repetitions for the provided <paramref name="item"/> in this set.
    /// </summary>
    /// <param name="item">Element to update.</param>
    /// <param name="value">Number of repetitions.</param>
    /// <returns>Previous multiplicity of the updated <paramref name="item"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="value"/> is less than <b>0</b>.</exception>
    /// <remarks>Setting multiplicity to <b>0</b> is equivalent to <see cref="RemoveAll(T)"/> invocation.</remarks>
    int SetMultiplicity(T item, int value);

    /// <inheritdoc cref="ICollection{T}.Contains(T)" />
    [Pure]
    new bool Contains(Pair<T, int> item);

    /// <inheritdoc cref="ISet{T}.IsProperSubsetOf(IEnumerable{T})" />
    [Pure]
    new bool IsProperSubsetOf(IEnumerable<Pair<T, int>> other);

    /// <inheritdoc cref="ISet{T}.IsProperSupersetOf(IEnumerable{T})" />
    [Pure]
    new bool IsProperSupersetOf(IEnumerable<Pair<T, int>> other);

    /// <inheritdoc cref="ISet{T}.IsSubsetOf(IEnumerable{T})" />
    [Pure]
    new bool IsSubsetOf(IEnumerable<Pair<T, int>> other);

    /// <inheritdoc cref="ISet{T}.IsSupersetOf(IEnumerable{T})" />
    [Pure]
    new bool IsSupersetOf(IEnumerable<Pair<T, int>> other);

    /// <inheritdoc cref="ISet{T}.Overlaps(IEnumerable{T})" />
    [Pure]
    new bool Overlaps(IEnumerable<Pair<T, int>> other);

    /// <inheritdoc cref="ISet{T}.SetEquals(IEnumerable{T})" />
    [Pure]
    new bool SetEquals(IEnumerable<Pair<T, int>> other);
}
