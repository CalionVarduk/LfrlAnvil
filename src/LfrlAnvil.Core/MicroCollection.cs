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
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Represents a collection of objects optimized for zero or one element(s).
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct MicroCollection<T>
{
    private T[] _tail;
    private T? _head;

    private MicroCollection(int count)
    {
        Assume.Equals( count, 0 );
        _tail = Array.Empty<T>();
        _head = default;
        Count = 0;
    }

    /// <summary>
    /// Gets the number of elements contained in this collection.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets an element at the given index.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public T this[int index]
    {
        get
        {
            Ensure.IsInIndexRange( index, Count );
            return index == 0 ? _head! : _tail[index - 1];
        }
        set
        {
            Ensure.IsInIndexRange( index, Count );
            if ( index == 0 )
                _head = value;
            else
                _tail[index - 1] = value;
        }
    }

    /// <summary>
    /// Creates a new empty <see cref="MicroCollection{T}"/> instance.
    /// </summary>
    /// <returns>New <see cref="MicroCollection{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static MicroCollection<T> Create()
    {
        return new MicroCollection<T>( 0 );
    }

    /// <summary>
    /// Searches for the specified <paramref name="item"/> in this collection and returns the index of its first occurrence.
    /// </summary>
    /// <param name="item">Item to search for.</param>
    /// <returns>
    /// The zero-based index of the first occurrence of <paramref name="item"/>, if it exists in this collection, otherwise <b>-1</b>.
    /// </returns>
    [Pure]
    public int IndexOf(T item)
    {
        if ( Count == 0 )
            return -1;

        if ( EqualityComparer<T>.Default.Equals( _head, item ) )
            return 0;

        var index = Array.IndexOf( _tail, item, 0, Count - 1 );
        return index >= 0 ? index + 1 : -1;
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the end of this collection.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void Add(T item)
    {
        if ( Count > 0 )
        {
            var nextCount = checked( Count + 1 );
            if ( _tail.Length < Count )
            {
                var prevTail = _tail;
                _tail = new T[Buffers.GetCapacity( Count )];
                prevTail.AsSpan().CopyTo( _tail );
            }

            _tail[Count - 1] = item;
            Count = nextCount;
            return;
        }

        Count = 1;
        _head = item;
    }

    /// <summary>
    /// Removes the first occurrence of <paramref name="item"/> from this collection.
    /// </summary>
    /// <param name="item">Item to remove.</param>
    /// <returns><b>true</b> when <paramref name="item"/> occurrence was removed successfully, otherwise <b>false</b>.</returns>
    public bool Remove(T item)
    {
        if ( Count == 0 )
            return false;

        if ( EqualityComparer<T>.Default.Equals( _head, item ) )
        {
            if ( --Count > 0 )
            {
                _head = _tail[0];
                if ( Count == 1 )
                    _tail = Array.Empty<T>();
                else
                {
                    _tail.AsSpan( 1, Count - 1 ).CopyTo( _tail );
                    _tail[Count - 1] = default!;
                }
            }
            else
            {
                _head = default;
                _tail = Array.Empty<T>();
            }

            return true;
        }

        var index = Array.IndexOf( _tail, item, 0, Count - 1 );
        if ( index < 0 )
            return false;

        if ( --Count > 1 )
        {
            var count = Count - index - 1;
            _tail.AsSpan( index + 1, count ).CopyTo( _tail.AsSpan( index, count ) );
            _tail[Count - 1] = default!;
        }
        else
            _tail = Array.Empty<T>();

        return true;
    }

    /// <summary>
    /// Removes all elements from this collection.
    /// </summary>
    public void Clear()
    {
        _head = default;
        _tail = Array.Empty<T>();
        Count = 0;
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this collection.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _head, _tail, Count - 1 );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="MicroCollection{T}"/>.
    /// </summary>
    public struct Enumerator
    {
        private readonly T[] _tail;
        private readonly T? _head;
        private readonly int _count;
        private int _index;

        internal Enumerator(T? head, T[] tail, int count)
        {
            _head = head;
            _tail = tail;
            _count = count;
            _index = -2;
        }

        /// <summary>
        /// Gets the element at the current position of the enumerator.
        /// </summary>
        public T Current => _index == -1 ? _head! : _tail[_index];

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return ++_index < _count;
        }
    }
}
