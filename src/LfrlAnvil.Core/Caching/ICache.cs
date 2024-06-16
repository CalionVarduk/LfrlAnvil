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
using System.Diagnostics.CodeAnalysis;

namespace LfrlAnvil.Caching;

/// <summary>
/// Represents a generic cache of keyed entries.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
public interface ICache<TKey, TValue> : IReadOnlyCache<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Gets or sets the entry that has the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">The key of the entry to get or set.</param>
    /// <exception cref="KeyNotFoundException">Entry with the provided <paramref name="key"/> does not exist.</exception>
    /// <remarks>See <see cref="AddOrUpdate(TKey,TValue)"/> for more information about the setter's behavior.</remarks>
    new TValue this[TKey key] { get; set; }

    /// <summary>
    /// Attempts to add a new entry.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    /// <returns><b>true</b> when entry has been added (provided <paramref name="key"/> did not exist), otherwise <b>false</b>.</returns>
    /// <remarks>
    /// When new entry addition causes <see cref="IReadOnlyCollection{T}.Count"/>
    /// to exceed <see cref="IReadOnlyCache{TKey,TValue}.Capacity"/>,
    /// then the <see cref="IReadOnlyCache{TKey,TValue}.Oldest"/> entry will be removed automatically.
    /// </remarks>
    bool TryAdd(TKey key, TValue value);

    /// <summary>
    /// Adds a new entry or updates an existing one if <paramref name="key"/> already exists.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    /// <returns>
    /// <see cref="AddOrUpdateResult.Added"/> when new entry has been added (provided <paramref name="key"/> did not exist),
    /// otherwise <see cref="AddOrUpdateResult.Updated"/>.
    /// </returns>
    /// <remarks>
    /// When new entry addition causes <see cref="IReadOnlyCollection{T}.Count"/>
    /// to exceed <see cref="IReadOnlyCache{TKey,TValue}.Capacity"/>,
    /// then the <see cref="IReadOnlyCache{TKey,TValue}.Oldest"/> entry will be removed automatically.
    /// </remarks>
    AddOrUpdateResult AddOrUpdate(TKey key, TValue value);

    /// <summary>
    /// Attempts to remove an entry with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key of an entry to remove.</param>
    /// <returns><b>true</b> when entry has been removed, otherwise <b>false</b>.</returns>
    bool Remove(TKey key);

    /// <summary>
    /// Attempts to remove an entry with the specified <paramref name="key"/> and to return a value associated with that key.
    /// </summary>
    /// <param name="key">Key of an entry to remove.</param>
    /// <param name="removed">An <b>out</b> parameter that returns a value associated with the <paramref name="key"/>, if it exists.</param>
    /// <returns><b>true</b> when entry has been removed, otherwise <b>false</b>.</returns>
    bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed);

    /// <summary>
    /// Attempts to restart an entry associated with the specified <paramref name="key"/> by e.g. moving it to the top of the cache.
    /// </summary>
    /// <param name="key">Key of an entry to restart.</param>
    /// <returns><b>true</b> when entry has been restarted (provided <paramref name="key"/> exists), otherwise <b>false</b>.</returns>
    bool Restart(TKey key);

    /// <summary>
    /// Removes all entries from the collection.
    /// </summary>
    void Clear();
}
