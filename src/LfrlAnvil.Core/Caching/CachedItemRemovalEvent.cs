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
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Caching;

/// <summary>
/// A lightweight representation of an event that signals removal of an entry from an <see cref="ICache{TKey,TValue}"/>.
/// </summary>
/// <param name="Key">Key of an entry associated with this event.</param>
/// <param name="Removed">Value of a removed entry associated with this event.</param>
/// <param name="Replacement">Value of an entry that replaced the <see cref="Removed"/> value associated with this event.</param>
/// <param name="IsReplaced">Indicates whether or not this event contains a <see cref="Replacement"/> value.</param>
/// <typeparam name="TKey">Cache entry's key (identifier) type.</typeparam>
/// <typeparam name="TValue">Cache entry's value type.</typeparam>
public readonly record struct CachedItemRemovalEvent<TKey, TValue>(TKey Key, TValue Removed, TValue? Replacement, bool IsReplaced)
{
    /// <summary>
    /// Creates a new <see cref="CachedItemRemovalEvent{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="key">Key of the removed cache entry.</param>
    /// <param name="removed">Value of the removed cache entry.</param>
    /// <returns><see cref="CachedItemRemovalEvent{TKey,TValue}"/> instance without a <see cref="Replacement"/> value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CachedItemRemovalEvent<TKey, TValue> CreateRemoved(TKey key, TValue removed)
    {
        return new CachedItemRemovalEvent<TKey, TValue>( key, removed, default, false );
    }

    /// <summary>
    /// Creates a new <see cref="CachedItemRemovalEvent{TKey,TValue}"/> instance with <see cref="Replacement"/> value.
    /// </summary>
    /// <param name="key">Key of the cache entry.</param>
    /// <param name="removed">Removed value associated with the <paramref name="key"/>.</param>
    /// <param name="replacement">
    /// Value associated with the <paramref name="key"/> which replaced the <paramref name="removed"/> value.
    /// </param>
    /// <returns><see cref="CachedItemRemovalEvent{TKey,TValue}"/> instance with a <see cref="Replacement"/> value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static CachedItemRemovalEvent<TKey, TValue> CreateReplaced(TKey key, TValue removed, TValue replacement)
    {
        return new CachedItemRemovalEvent<TKey, TValue>( key, removed, replacement, true );
    }
}
