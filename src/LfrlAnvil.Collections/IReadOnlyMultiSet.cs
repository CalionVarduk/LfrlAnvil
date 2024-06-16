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

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic read-only multi set.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public interface IReadOnlyMultiSet<T> : IReadOnlySet<Pair<T, int>>
    where T : notnull
{
    /// <summary>
    /// Gets the full count of all elements, including repetitions.
    /// </summary>
    long FullCount { get; }

    /// <summary>
    /// Gets all elements, including repetitions.
    /// </summary>
    IEnumerable<T> Items { get; }

    /// <summary>
    /// Gets all unique elements.
    /// </summary>
    IEnumerable<T> DistinctItems { get; }

    /// <summary>
    /// Element comparer.
    /// </summary>
    IEqualityComparer<T> Comparer { get; }

    /// <summary>
    /// Checks whether or not the provided <paramref name="item"/> exists in this set.
    /// </summary>
    /// <param name="item">Element to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="item"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(T item);

    /// <summary>
    /// Checks whether or not the provided <paramref name="item"/> exists in this set with a minimum number of repetitions.
    /// </summary>
    /// <param name="item">Element to check.</param>
    /// <param name="multiplicity">Expected minimum number of repetitions.</param>
    /// <returns>
    /// <b>true</b> when the provided <paramref name="item"/> exists with a minimum number of repetitions, otherwise <b>false</b>.
    /// </returns>
    [Pure]
    bool Contains(T item, int multiplicity);

    /// <summary>
    /// Returns the number of repetitions associated with the provided <paramref name="item"/>.
    /// </summary>
    /// <param name="item">Element to get the number of repetitions for.</param>
    /// <returns>Number of <paramref name="item"/> repetitions.</returns>
    [Pure]
    int GetMultiplicity(T item);
}
