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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Memory;

/// <summary>
/// A lightweight container for an underlying <see cref="MemoryPool{T}"/> node.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly struct MemoryPoolToken<T> : IDisposable
{
    /// <summary>
    /// An empty sequence.
    /// </summary>
    public static MemoryPoolToken<T> Empty => new MemoryPoolToken<T>( null, 0, false );

    private readonly int _nodeId;

    internal MemoryPoolToken(MemoryPool<T>? owner, int nodeId, bool clear)
    {
        _nodeId = nodeId;
        Clear = clear;
        Owner = owner;
    }

    /// <summary>
    /// Specifies whether or not the underlying buffer will be additionally cleared during this token's disposal.
    /// </summary>
    public bool Clear { get; }

    /// <summary>
    /// <see cref="MemoryPool{T}"/> instance that owns this token.
    /// </summary>
    public MemoryPool<T>? Owner { get; }

    /// <inheritdoc />
    /// <remarks>Frees the underlying node and returns it to the pool.</remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        Owner?.Release( _nodeId, Clear );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="MemoryPoolToken{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{MemoryPool<T>.GetLengthString( Owner, _nodeId )}{(Clear ? " (clear enabled)" : string.Empty)}";
    }

    /// <summary>
    /// Creates a new <see cref="MemoryPoolToken{T}"/> from this token with a new <see cref="Clear"/> value.
    /// </summary>
    /// <param name="enabled"><see cref="Clear"/> value to set.</param>
    /// <returns>New <see cref="MemoryPoolToken{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public MemoryPoolToken<T> EnableClearing(bool enabled = true)
    {
        return new MemoryPoolToken<T>( Owner, _nodeId, enabled );
    }

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> instance from the underlying buffer of this token.
    /// </summary>
    /// <returns>New <see cref="Memory{T}"/> instance.</returns>
    /// <remarks>Returns an <see cref="Memory{T}.Empty"/> instance for disposed tokens.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Memory<T> AsMemory()
    {
        return Owner?.AsMemory( _nodeId ) ?? Memory<T>.Empty;
    }

    /// <summary>
    /// Creates a new <see cref="Span{T}"/> instance from the underlying buffer of this token.
    /// </summary>
    /// <returns>New <see cref="Span{T}"/> instance.</returns>
    /// <remarks>Returns an <see cref="Span{T}.Empty"/> instance for disposed tokens.</remarks>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Span<T> AsSpan()
    {
        return Owner is not null ? Owner.AsSpan( _nodeId ) : Span<T>.Empty;
    }

    /// <summary>
    /// Resizes the underlying buffer.
    /// Length change will invalidate exposed underlying buffer's <see cref="Memory{T}"/> or <see cref="Span{T}"/> instances.
    /// </summary>
    /// <param name="length">New underlying buffer length.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="length"/> is less than or equal to <b>0</b>.</exception>
    /// <remarks>
    /// When new <paramref name="length"/> is less than the current length, then elements at the end of the buffer will be discarded.
    /// Additionally, the <see cref="Clear"/> value will determine whether or not discarded elements will be cleared.
    /// </remarks>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void SetLength(int length)
    {
        Owner?.SetLength( _nodeId, length, Clear );
    }

    /// <summary>
    /// Returns a <see cref="MemoryPool{T}.ReportInfo.Node"/> instance that represents this token, if node exists.
    /// </summary>
    /// <returns>New <see cref="MemoryPool{T}.ReportInfo.Node"/> instance or <b>null</b>, if node does not exist.</returns>
    [Pure]
    public MemoryPool<T>.ReportInfo.Node? TryGetInfo()
    {
        return Owner?.TryGetInfo( _nodeId );
    }
}
