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

using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

/// <summary>
/// Represents a generic cache of keyed entries with a limited lifetime.
/// </summary>
/// <typeparam name="TKey">Entry key (identifier) type.</typeparam>
/// <typeparam name="TValue">Entry value type.</typeparam>
/// <remarks>Reading entries resets their lifetime.</remarks>
public interface ILifetimeCache<TKey, TValue> : IReadOnlyLifetimeCache<TKey, TValue>, ICache<TKey, TValue>
    where TKey : notnull
{
    /// <summary>
    /// Moves this cache forward in time and removes entries with elapsed lifetimes.
    /// </summary>
    /// <param name="delta"><see cref="Duration"/> to add to the <see cref="IReadOnlyLifetimeCache{TKey,TValue}.CurrentTimestamp"/>.</param>
    void Move(Duration delta);
}
