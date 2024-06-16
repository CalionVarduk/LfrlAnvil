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
using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

/// <summary>
/// Represents a generic cache of keyed entries with a limited lifetime, where each entry can have its own individual lifetime.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
/// <remarks>Reading entries resets their lifetime.</remarks>
public interface IIndividualLifetimeCache<TKey, TValue> : ILifetimeCache<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Attempts to add a new entry.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    /// <param name="lifetime">Entry's lifetime.</param>
    /// <returns><b>true</b> when entry has been added (provided <paramref name="key"/> did not exist), otherwise <b>false</b>.</returns>
    /// <remarks>
    /// When new entry addition causes <see cref="IReadOnlyCollection{T}.Count"/>
    /// to exceed <see cref="IReadOnlyCache{TKey,TValue}.Capacity"/>,
    /// then the <see cref="IReadOnlyCache{TKey,TValue}.Oldest"/> entry will be removed automatically.
    /// </remarks>
    bool TryAdd(TKey key, TValue value, Duration lifetime);

    /// <summary>
    /// Adds a new entry or updates an existing one if <paramref name="key"/> already exists.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="value">Entry's value.</param>
    /// <param name="lifetime">Entry's lifetime.</param>
    /// <returns>
    /// <see cref="AddOrUpdateResult.Added"/> when new entry has been added (provided <paramref name="key"/> did not exist),
    /// otherwise <see cref="AddOrUpdateResult.Updated"/>.
    /// </returns>
    /// <remarks>
    /// When new entry addition causes <see cref="IReadOnlyCollection{T}.Count"/>
    /// to exceed <see cref="IReadOnlyCache{TKey,TValue}.Capacity"/>,
    /// then the <see cref="IReadOnlyCache{TKey,TValue}.Oldest"/> entry will be removed automatically.
    /// </remarks>
    AddOrUpdateResult AddOrUpdate(TKey key, TValue value, Duration lifetime);
}
