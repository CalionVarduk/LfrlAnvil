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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

/// <summary>
/// A lightweight generic read-only container for an <see cref="Array"/>.
/// </summary>
/// <typeparam name="T"></typeparam>
public readonly struct ReadOnlyArray<T> : IReadOnlyList<T>
{
    /// <summary>
    /// Represents a empty read-only <see cref="Array"/>.
    /// </summary>
    public static readonly ReadOnlyArray<T> Empty = new ReadOnlyArray<T>( Array.Empty<T>() );

    private readonly T[] _source;

    /// <summary>
    /// Creates a new <see cref="ReadOnlyArray{T}"/> instance.
    /// </summary>
    /// <param name="source">Underlying <see cref="Array"/>.</param>
    public ReadOnlyArray(T[] source)
    {
        _source = source;
    }

    /// <inheritdoc />
    public int Count => _source.Length;

    /// <inheritdoc />
    public T this[int index] => _source[index];

    /// <summary>
    /// Creates a new <see cref="ReadOnlyArray{T}"/> instance.
    /// </summary>
    /// <param name="source">Underlying array.</param>
    /// <typeparam name="TSource">Array element type convertible to read-only array's element type.</typeparam>
    /// <returns>New <see cref="ReadOnlyArray{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlyArray<T> From<TSource>(TSource[] source)
        where TSource : class?, T
    {
        return new ReadOnlyArray<T>( source );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlyArray{T}"/> instance.
    /// </summary>
    /// <param name="source">Other read-only array.</param>
    /// <typeparam name="TSource">Array element type convertible to read-only array's element type.</typeparam>
    /// <returns>New <see cref="ReadOnlyArray{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlyArray<T> From<TSource>(ReadOnlyArray<TSource> source)
        where TSource : class?, T
    {
        return From( source._source );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> instance from this <see cref="ReadOnlyArray{T}"/>.
    /// </summary>
    /// <returns>New <see cref="ReadOnlyMemory{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlyMemory<T> AsMemory()
    {
        return _source;
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlySpan{T}"/> instance from this <see cref="ReadOnlyArray{T}"/>.
    /// </summary>
    /// <returns>New <see cref="ReadOnlySpan{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<T> AsSpan()
    {
        return _source;
    }

    /// <summary>
    /// Returns the underlying array.
    /// </summary>
    /// <returns>The underlying array.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IReadOnlyList<T> GetUnderlyingArray()
    {
        return _source;
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this sequence.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    /// <summary>
    /// Converts the provided <paramref name="source"/> to <see cref="ReadOnlyArray{T}"/>.
    /// </summary>
    /// <param name="source">Underlying array.</param>
    /// <returns>New <see cref="ReadOnlyArray{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator ReadOnlyArray<T>(T[] source)
    {
        return new ReadOnlyArray<T>( source );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="ReadOnlyArray{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] _source;
        private int _index;

        internal Enumerator(T[] source)
        {
            _source = source;
            _index = -1;
        }

        /// <inheritdoc />
        public T Current => _source[_index];

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            var i = _index + 1;
            if ( i >= _source.Length )
                return false;

            _index = i;
            return true;
        }

        /// <inheritdoc />
        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
