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
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic heap data structure.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public interface IHeap<T> : IReadOnlyHeap<T>
{
    /// <summary>
    /// Removes and returns an entry currently at the top of this heap.
    /// </summary>
    /// <returns>Removed entry.</returns>
    /// <exception cref="IndexOutOfRangeException">When this heap is empty.</exception>
    T Extract();

    /// <summary>
    /// Attempt to remove and return an entry currently at the top of this heap if it is not empty.
    /// </summary>
    /// <param name="result"><b>out</b> parameter that returns the removed entry.</param>
    /// <returns><b>true</b> when this heap was not empty and entry has been removed, otherwise <b>false</b>.</returns>
    bool TryExtract([MaybeNullWhen( false )] out T result);

    /// <summary>
    /// Adds a new entry to this heap.
    /// </summary>
    /// <param name="item">Entry to add.</param>
    void Add(T item);

    /// <summary>
    /// Removes an entry currently at the top of this heap.
    /// </summary>
    /// <exception cref="IndexOutOfRangeException">When this heap is empty.</exception>
    void Pop();

    /// <summary>
    /// Attempts to remove an entry currently at the top of the heap if it is not empty.
    /// </summary>
    /// <returns><b>true</b> when this heap was not empty and entry has been removed, otherwise <b>false</b>.</returns>
    bool TryPop();

    /// <summary>
    /// Returns and replaces an entry currently at the top of the heap with a new entry.
    /// </summary>
    /// <param name="item">Replacement entry.</param>
    /// <returns>Removed entry.</returns>
    /// <exception cref="IndexOutOfRangeException">When this heap is empty.</exception>
    T Replace(T item);

    /// <summary>
    /// Attempt to return and replace an entry currently at the top of this heap with a new entry if it is not empty.
    /// </summary>
    /// <param name="item">Replacement entry.</param>
    /// <param name="replaced"><b>out</b> parameter that returns the removed entry.</param>
    /// <returns><b>true</b> when this heap is not empty and entry has been replaced, otherwise <b>false</b>.</returns>
    bool TryReplace(T item, [MaybeNullWhen( false )] out T replaced);

    /// <summary>
    /// Removes all entries from this heap.
    /// </summary>
    void Clear();
}
