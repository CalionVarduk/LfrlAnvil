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

using System.Diagnostics.Contracts;
using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

/// <summary>
/// Represents a generic read-only cache of keyed entries with a limited lifetime.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
/// <remarks>Reading entries resets their lifetime.</remarks>
public interface IReadOnlyLifetimeCache<TKey, TValue> : IReadOnlyCache<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Lifetime of added entries.
    /// </summary>
    Duration Lifetime { get; }

    /// <summary>
    /// <see cref="Timestamp"/> of the creation of this cache.
    /// </summary>
    Timestamp StartTimestamp { get; }

    /// <summary>
    /// <see cref="Timestamp"/> at which this cache currently is.
    /// </summary>
    Timestamp CurrentTimestamp { get; }

    /// <summary>
    /// Gets the remaining lifetime of an entry with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>Entry's lifetime or <see cref="Duration.Zero"/> when key does not exist.</returns>
    [Pure]
    Duration GetRemainingLifetime(TKey key);
}
