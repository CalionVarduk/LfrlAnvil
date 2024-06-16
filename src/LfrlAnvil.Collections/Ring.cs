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
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="IRing{T}" />
public class Ring<T> : IRing<T>
{
    private int _writeIndex;
    private readonly T?[] _items;

    /// <summary>
    /// Creates a new <see cref="Ring{T}"/> instance.
    /// </summary>
    /// <param name="count">Number of elements.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="count"/> is less than <b>1</b>.</exception>
    public Ring(int count)
    {
        Ensure.IsGreaterThan( count, 0 );
        _items = new T?[count];
        _writeIndex = 0;
    }

    /// <summary>
    /// Creates a new <see cref="Ring{T}"/> instance from the provided collection.
    /// </summary>
    /// <param name="range">Initial collection of elements.</param>
    /// <exception cref="ArgumentOutOfRangeException">When collection is empty.</exception>
    public Ring(IEnumerable<T?> range)
    {
        _items = range.ToArray();
        Ensure.IsGreaterThan( _items.Length, 0 );
        _writeIndex = 0;
    }

    /// <summary>
    /// Creates a new <see cref="Ring{T}"/> instance from the provided collection.
    /// </summary>
    /// <param name="range">Initial collection of elements.</param>
    /// <exception cref="ArgumentOutOfRangeException">When collection is empty.</exception>
    public Ring(params T?[] range)
        : this( range.AsEnumerable() ) { }

    /// <inheritdoc cref="IRing{T}.this" />
    public T? this[int index]
    {
        get => _items[index];
        set => _items[index] = value;
    }

    /// <inheritdoc />
    public int Count => _items.Length;

    /// <inheritdoc cref="IRing{T}.WriteIndex" />
    public int WriteIndex
    {
        get => _writeIndex;
        set => _writeIndex = GetWrappedIndex( value );
    }

    /// <inheritdoc />
    [Pure]
    public int GetWrappedIndex(int index)
    {
        return index.EuclidModulo( _items.Length );
    }

    /// <inheritdoc />
    [Pure]
    public int GetWriteIndex(int offset)
    {
        return GetWrappedIndex( _writeIndex + offset );
    }

    /// <inheritdoc />
    public void SetNext(T item)
    {
        _items[_writeIndex] = item;

        if ( ++_writeIndex == _items.Length )
            _writeIndex = 0;
    }

    /// <inheritdoc />
    public void Clear()
    {
        for ( var i = 0; i < _items.Length; ++i )
            _items[i] = default;

        _writeIndex = 0;
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerable<T?> Read(int readIndex)
    {
        using var enumerator = new Enumerator( _items, GetWrappedIndex( readIndex ) );

        while ( enumerator.MoveNext() )
            yield return enumerator.Current;
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this ring.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _items, GetWrappedIndex( _writeIndex ) );
    }

    [Pure]
    IEnumerator<T?> IEnumerable<T?>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="Ring{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T?>
    {
        private int _index;
        private int _stepsLeft;
        private readonly T?[] _items;

        internal Enumerator(T?[] items, int startIndex)
        {
            _items = items;
            _index = (startIndex == 0 ? _items.Length : startIndex) - 1;
            _stepsLeft = _items.Length;
        }

        /// <inheritdoc />
        public T? Current => _items[_index];

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public void Dispose() { }

        /// <inheritdoc />
        public bool MoveNext()
        {
            if ( _stepsLeft <= 0 )
                return false;

            --_stepsLeft;
            if ( ++_index == _items.Length )
                _index = 0;

            return true;
        }

        void IEnumerator.Reset()
        {
            _index -= _items.Length - _stepsLeft;

            if ( _index < 0 )
                _index += _items.Length;

            _stepsLeft = _items.Length;
        }
    }
}
