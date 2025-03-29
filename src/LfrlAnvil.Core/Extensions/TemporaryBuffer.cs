// Copyright 2025 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Extensions;

/// <summary>
/// Represents a lightweight result of a <see cref="EnumerableExtensions.BufferUntil{T}(IEnumerable{T},Func{T,T,Boolean},int)"/> operation.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
/// <remarks>
/// Buffers will not hold elements in the underlying buffer indefinitely, their underlying buffers will be reused as soon as possible.
/// </remarks>
public readonly struct TemporaryBuffer<T>
{
    private readonly T[]? _items;

    internal TemporaryBuffer(T[] items, int length)
    {
        _items = items;
        Length = length;
    }

    /// <summary>
    /// Gets the number of buffered elements.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Creates a new array from this buffer.
    /// </summary>
    /// <returns>New array.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public T[] ToArray()
    {
        if ( Length == 0 )
            return Array.Empty<T>();

        Assume.IsNotNull( _items );
        var result = new T[Length];
        Array.Copy( _items, result, Length );
        return result;
    }

    /// <summary>
    /// Creates a new enumerable object from this buffer.
    /// </summary>
    /// <returns>New enumerable object.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IEnumerable<T> AsEnumerable()
    {
        if ( Length == 0 )
            return Array.Empty<T>();

        Assume.IsNotNull( _items );
        return _items.Take( Length );
    }

    /// <summary>
    /// Creates a new read-only memory from this buffer.
    /// </summary>
    /// <returns>New <see cref="ReadOnlyMemory{T}"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlyMemory<T> AsMemory()
    {
        return _items.AsMemory( 0, Length );
    }

    /// <summary>
    /// Creates a new read-only span from this buffer.
    /// </summary>
    /// <returns>New <see cref="ReadOnlySpan{T}"/>.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<T> AsSpan()
    {
        return _items.AsSpan( 0, Length );
    }

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<T>.Enumerator GetEnumerator()
    {
        return AsSpan().GetEnumerator();
    }
}
