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
using System.Buffers;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Memory;

/// <summary>
/// Represents a disposable token that contains a buffer rented from an <see cref="ArrayPool{T}"/> instance.
/// </summary>
/// <typeparam name="T">The type of the objects that are in the resource pool.</typeparam>
public readonly struct ArrayPoolToken<T> : IDisposable
{
    private readonly T[]? _source;

    internal ArrayPoolToken(ArrayPool<T> pool, T[] source, int length, bool clearArray)
    {
        _source = source;
        Pool = pool;
        Length = length;
        ClearArray = clearArray;
    }

    /// <summary>
    /// Source resource pool.
    /// </summary>
    public ArrayPool<T>? Pool { get; }

    /// <summary>
    /// Requested buffer length.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Indicates whether the contents of the buffer should be cleared before reuse, when this token gets disposed.
    /// </summary>
    public bool ClearArray { get; }

    /// <summary>
    /// Rented buffer.
    /// </summary>
    public T[] Source => _source ?? Array.Empty<T>();

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( Pool is null )
            return;

        Assume.IsNotNull( _source );
        Pool.Return( _source, ClearArray );
    }

    /// <summary>
    /// Creates a new <see cref="Span{T}"/> instance from this token.
    /// </summary>
    /// <returns>New <see cref="Span{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Span<T> AsSpan()
    {
        return _source.AsSpan( 0, Length );
    }

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> instance from this token.
    /// </summary>
    /// <returns>New <see cref="Memory{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Memory<T> AsMemory()
    {
        return _source.AsMemory( 0, Length );
    }
}
