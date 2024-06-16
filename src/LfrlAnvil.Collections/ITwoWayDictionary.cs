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

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic collection of two-way (forward, reverse) pairs.
/// </summary>
/// <typeparam name="T1">First value type.</typeparam>
/// <typeparam name="T2">Second value type.</typeparam>
public interface ITwoWayDictionary<T1, T2> : IReadOnlyTwoWayDictionary<T1, T2>, ICollection<Pair<T1, T2>>
    where T1 : notnull
    where T2 : notnull
{
    /// <inheritdoc cref="ICollection{T}.Count" />
    new int Count { get; }

    /// <summary>
    /// Attempts to add the provided pair.
    /// </summary>
    /// <param name="first">First value.</param>
    /// <param name="second">Second value.</param>
    /// <returns><b>true</b> when pair was added, otherwise <b>false</b>.</returns>
    bool TryAdd(T1 first, T2 second);

    /// <summary>
    /// Adds the provided pair.
    /// </summary>
    /// <param name="first">First value.</param>
    /// <param name="second">Second value.</param>
    /// <exception cref="ArgumentException">When either element already exists as key in its respective dictionary.</exception>
    void Add(T1 first, T2 second);

    /// <summary>
    /// Attempts to update the value associated with the specified <paramref name="first"/> key.
    /// </summary>
    /// <param name="first">Entry's key.</param>
    /// <param name="second">Second value to set.</param>
    /// <returns><b>true</b> when pair was updated, otherwise <b>false</b>.</returns>
    bool TryUpdateForward(T1 first, T2 second);

    /// <summary>
    /// Updates the value associated with the specified <paramref name="first"/> key.
    /// </summary>
    /// <param name="first">Entry's key.</param>
    /// <param name="second">Second value to set.</param>
    /// <exception cref="ArgumentException">When the provided <paramref name="second"/> already exists as a key.</exception>
    void UpdateForward(T1 first, T2 second);

    /// <summary>
    /// Attempts to update the value associated with the specified <paramref name="second"/> key.
    /// </summary>
    /// <param name="second">Entry's key.</param>
    /// <param name="first">First value to set.</param>
    /// <returns><b>true</b> when pair was updated, otherwise <b>false</b>.</returns>
    bool TryUpdateReverse(T2 second, T1 first);

    /// <summary>
    /// Updates the value associated with the specified <paramref name="second"/> key.
    /// </summary>
    /// <param name="second">Entry's key.</param>
    /// <param name="first">First value to set.</param>
    /// <exception cref="ArgumentException">When the provided <paramref name="first"/> already exists as a key.</exception>
    void UpdateReverse(T2 second, T1 first);

    /// <summary>
    /// Attempts to remove a pair by its first <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Entry's key.</param>
    /// <returns><b>true</b> when pair was removed, otherwise <b>false</b>.</returns>
    bool RemoveForward(T1 value);

    /// <summary>
    /// Attempts to remove a pair by its second <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Entry's key.</param>
    /// <returns><b>true</b> when pair was removed, otherwise <b>false</b>.</returns>
    bool RemoveReverse(T2 value);

    /// <summary>
    /// Attempts to remove a pair by its first <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Entry's key.</param>
    /// <param name="second"><b>out</b> parameter that returns second value associated with the removed pair.</param>
    /// <returns><b>true</b> when pair was removed, otherwise <b>false</b>.</returns>
    bool RemoveForward(T1 value, [MaybeNullWhen( false )] out T2 second);

    /// <summary>
    /// Attempts to remove a pair by its second <paramref name="value"/>.
    /// </summary>
    /// <param name="value">Entry's key.</param>
    /// <param name="first"><b>out</b> parameter that returns first value associated with the removed pair.</param>
    /// <returns><b>true</b> when pair was removed, otherwise <b>false</b>.</returns>
    bool RemoveReverse(T2 value, [MaybeNullWhen( false )] out T1 first);
}
