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

using System.Buffers;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Memory;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Contains <see cref="ArrayPool{T}"/> extension methods.
/// </summary>
public static class ArrayPoolExtensions
{
    /// <summary>
    /// Retrieves a token that contains a buffer, that is at least the requested length.
    /// </summary>
    /// <param name="pool">Source resource pool.</param>
    /// <param name="minimumLength">The minimum length of the array.</param>
    /// <param name="clearArray">
    /// Indicates whether the contents of the buffer should be cleared before reuse, when token gets disposed.
    /// </param>
    /// <typeparam name="T">The type of the objects that are in the resource pool.</typeparam>
    /// <returns>New <see cref="ArrayPoolToken{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ArrayPoolToken<T> RentToken<T>(this ArrayPool<T> pool, int minimumLength, bool clearArray = false)
    {
        Ensure.IsGreaterThanOrEqualTo( minimumLength, 0 );
        var source = pool.Rent( minimumLength );
        return new ArrayPoolToken<T>( pool, source, minimumLength, clearArray );
    }
}
