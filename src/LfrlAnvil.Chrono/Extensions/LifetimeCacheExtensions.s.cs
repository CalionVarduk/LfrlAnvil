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

using System.Runtime.CompilerServices;
using LfrlAnvil.Chrono.Caching;

namespace LfrlAnvil.Chrono.Extensions;

/// <summary>
/// Contains <see cref="ILifetimeCache{TKey,TValue}"/> extension methods.
/// </summary>
public static class LifetimeCacheExtensions
{
    /// <summary>
    /// Moves the provided <paramref name="cache"/> to the given <paramref name="timestamp"/>.
    /// </summary>
    /// <param name="cache">Source cache.</param>
    /// <param name="timestamp"><see cref="Timestamp"/> to move the cache to.</param>
    /// <typeparam name="TKey">Cache's key type.</typeparam>
    /// <typeparam name="TValue">Cache's value type.</typeparam>
    /// <remarks>See <see cref="ILifetimeCache{TKey,TValue}.Move(Duration)"/> for more information.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static void MoveTo<TKey, TValue>(this ILifetimeCache<TKey, TValue> cache, Timestamp timestamp)
        where TKey : notnull
    {
        cache.Move( timestamp - cache.CurrentTimestamp );
    }
}
