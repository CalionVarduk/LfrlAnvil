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
/// Represents a generic read-only collection of two-way (forward, reverse) pairs.
/// </summary>
/// <typeparam name="T1">First value type.</typeparam>
/// <typeparam name="T2">Second value type.</typeparam>
public interface IReadOnlyTwoWayDictionary<T1, T2> : IReadOnlyCollection<Pair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    /// <summary>
    /// Represents the <typeparamref name="T1"/> => <typeparamref name="T2"/> read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<T1, T2> Forward { get; }

    /// <summary>
    /// Represents the <typeparamref name="T2"/> => <typeparamref name="T1"/> read-only dictionary.
    /// </summary>
    IReadOnlyDictionary<T2, T1> Reverse { get; }

    /// <summary>
    /// Forward key equality comparer.
    /// </summary>
    IEqualityComparer<T1> ForwardComparer { get; }

    /// <summary>
    /// Reverse key equality comparer.
    /// </summary>
    IEqualityComparer<T2> ReverseComparer { get; }

    /// <summary>
    /// Checks whether or not the provided pair exists.
    /// </summary>
    /// <param name="first">First value.</param>
    /// <param name="second">Second value.</param>
    /// <returns><b>true</b> when pair exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(T1 first, T2 second);
}
