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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil;

/// <summary>
/// Represents a slim version of first-in, first-out collection of objects implemented as a circular buffer.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct QueueSlim<T>
{
    /// <summary>
    /// Minimum capacity for non-empty queues. Equal to <b>4</b>.
    /// </summary>
    public const int MinCapacity = 1 << 2;

    private T[] _items;
    private int _firstIndex;
    private int _endIndex;

    private QueueSlim(int minCapacity)
    {
        _items = minCapacity <= 0 ? Array.Empty<T>() : new T[GetCapacity( minCapacity )];
        _firstIndex = _endIndex = 0;
    }

    /// <summary>
    /// Gets the number of elements contained in this queue.
    /// </summary>
    public int Count
    {
        get
        {
            if ( _firstIndex < _endIndex )
                return _endIndex - _firstIndex;

            if ( _firstIndex > _endIndex )
                return _endIndex - (_firstIndex - _items.Length);

            if ( IsEmpty )
                return 0;

            Assume.IsGreaterThan( _firstIndex, 0 );
            return _items.Length;
        }
    }

    /// <summary>
    /// Gets the number of maximum elements that this queue can contain without resizing the underlying buffer.
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// Specifies whether or not this queue is empty.
    /// </summary>
    public bool IsEmpty => _endIndex == 0;

    /// <summary>
    /// Gets a reference to an element at the given index.
    /// </summary>
    /// <param name="index">The zero-based index of the element reference to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public ref T this[int index]
    {
        get
        {
            ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
            if ( _firstIndex < _endIndex )
            {
                Ensure.IsInIndexRange( index, _endIndex - _firstIndex );
                return ref Unsafe.Add( ref first, _firstIndex + index );
            }

            if ( _firstIndex > _endIndex )
            {
                Ensure.IsInIndexRange( index, _endIndex - (_firstIndex - _items.Length) );
                return ref Unsafe.Add( ref first, GetWrappedIndex( index ) );
            }

            if ( IsEmpty )
                Ensure.IsInIndexRange( index, 0 );

            Assume.Equals( Count, _items.Length );
            Ensure.IsInIndexRange( index, _items.Length );
            return ref Unsafe.Add( ref first, GetWrappedIndex( index ) );
        }
    }

    /// <summary>
    /// Creates a new empty <see cref="QueueSlim{T}"/> instance.
    /// </summary>
    /// <param name="minCapacity">Minimum initial <see cref="Capacity"/> of the created queue. Equal to <b>0</b> by default.</param>
    /// <returns>New <see cref="QueueSlim{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static QueueSlim<T> Create(int minCapacity = 0)
    {
        return new QueueSlim<T>( minCapacity );
    }

    /// <summary>
    /// Creates a new <see cref="QueueSlimMemory{T}"/> instance that represents a view of this queue.
    /// </summary>
    /// <returns>New <see cref="QueueSlimMemory{T}"/> instance.</returns>
    [Pure]
    public QueueSlimMemory<T> AsMemory()
    {
        if ( _firstIndex < _endIndex )
            return new QueueSlimMemory<T>( _items.AsMemory( _firstIndex, _endIndex - _firstIndex ), ReadOnlyMemory<T>.Empty );

        if ( _firstIndex >= _endIndex && ! IsEmpty )
            return new QueueSlimMemory<T>( _items.AsMemory( _firstIndex ), _items.AsMemory( 0, _endIndex ) );

        return new QueueSlimMemory<T>( _items.AsMemory( 0, 0 ), ReadOnlyMemory<T>.Empty );
    }

    /// <summary>
    /// Returns a reference to the first element.
    /// </summary>
    /// <returns>Reference to the first element.</returns>
    /// <remarks>May return an invalid reference, when this queue is empty.</remarks>
    [Pure]
    public ref T First()
    {
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        return ref Unsafe.Add( ref first, _firstIndex );
    }

    /// <summary>
    /// Returns a reference to the last element.
    /// </summary>
    /// <returns>Reference to the last element.</returns>
    /// <remarks>May return an invalid reference, when this queue is empty.</remarks>
    [Pure]
    public ref T Last()
    {
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        return ref Unsafe.Add( ref first, _endIndex - 1 );
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the tail of this queue.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void Enqueue(T item)
    {
        if ( _firstIndex < _endIndex )
        {
            if ( _endIndex < _items.Length )
                _items[_endIndex++] = item;
            else if ( _firstIndex > 0 )
            {
                _items[0] = item;
                _endIndex = 1;
            }
            else
            {
                var prevItems = _items;
                _items = new T[GetCapacity( checked( _items.Length + 1 ) )];
                prevItems.CopyTo( _items.AsSpan() );
                _items[_endIndex++] = item;
            }
        }
        else if ( _firstIndex > _endIndex )
            _items[_endIndex++] = item;
        else if ( IsEmpty )
        {
            if ( _items.Length == 0 )
                _items = new T[GetCapacity( MinCapacity )];

            _items[_endIndex++] = item;
        }
        else
        {
            Assume.Equals( Count, _items.Length );
            var prevItems = _items;
            var nextCount = checked( _items.Length + 1 );
            _items = new T[GetCapacity( nextCount )];
            var firstSlice = prevItems.AsSpan( _firstIndex );
            firstSlice.CopyTo( _items );
            prevItems.AsSpan( 0, _endIndex ).CopyTo( _items.AsSpan( firstSlice.Length ) );
            _items[prevItems.Length] = item;
            _firstIndex = 0;
            _endIndex = nextCount;
        }
    }

    /// <summary>
    /// Adds a range of <paramref name="items"/> to the tail of this queue.
    /// </summary>
    /// <param name="items">Items to add.</param>
    /// <remarks>Does nothing when <paramref name="items"/> are empty.</remarks>
    public void EnqueueRange(ReadOnlySpan<T> items)
    {
        if ( items.Length == 0 )
            return;

        if ( _firstIndex < _endIndex )
        {
            var count = _endIndex - _firstIndex;
            var remaining = _items.Length - count;
            if ( items.Length <= remaining )
            {
                var endCount = _items.Length - _endIndex;
                if ( items.Length <= endCount )
                {
                    items.CopyTo( _items.AsSpan( _endIndex ) );
                    _endIndex += items.Length;
                }
                else
                {
                    items.Slice( 0, endCount ).CopyTo( _items.AsSpan( _endIndex ) );
                    items.Slice( endCount ).CopyTo( _items );
                    _endIndex = items.Length - endCount;
                }
            }
            else
            {
                var prevItems = _items;
                var nextCount = checked( count + items.Length );
                _items = new T[GetCapacity( nextCount )];
                prevItems.AsSpan( _firstIndex, count ).CopyTo( _items );
                items.CopyTo( _items.AsSpan( count ) );
                _firstIndex = 0;
                _endIndex = nextCount;
            }
        }
        else if ( _firstIndex > _endIndex )
        {
            var count = _endIndex - (_firstIndex - _items.Length);
            var remaining = _items.Length - count;
            if ( items.Length <= remaining )
            {
                items.CopyTo( _items.AsSpan( _endIndex ) );
                _endIndex += items.Length;
            }
            else
            {
                var prevItems = _items;
                var nextCount = checked( count + items.Length );
                _items = new T[GetCapacity( nextCount )];
                var firstSlice = prevItems.AsSpan( _firstIndex );
                firstSlice.CopyTo( _items );
                prevItems.AsSpan( 0, _endIndex ).CopyTo( _items.AsSpan( firstSlice.Length ) );
                items.CopyTo( _items.AsSpan( count ) );
                _firstIndex = 0;
                _endIndex = nextCount;
            }
        }
        else if ( IsEmpty )
        {
            if ( _items.Length < items.Length )
                _items = new T[GetCapacity( items.Length )];

            items.CopyTo( _items );
            _endIndex = items.Length;
        }
        else
        {
            Assume.Equals( Count, _items.Length );
            var prevItems = _items;
            var nextCount = checked( _items.Length + items.Length );
            _items = new T[GetCapacity( nextCount )];
            var firstSlice = prevItems.AsSpan( _firstIndex );
            firstSlice.CopyTo( _items );
            prevItems.AsSpan( 0, _endIndex ).CopyTo( _items.AsSpan( firstSlice.Length ) );
            items.CopyTo( _items.AsSpan( prevItems.Length ) );
            _firstIndex = 0;
            _endIndex = nextCount;
        }
    }

    /// <summary>
    /// Attempts to remove the first item from this queue.
    /// </summary>
    /// <returns><b>true</b> when queue was not empty and first item was removed, otherwise <b>false</b>.</returns>
    public bool Dequeue()
    {
        if ( _firstIndex < _endIndex )
        {
            _items[_firstIndex++] = default!;
            if ( _firstIndex == _endIndex )
                _firstIndex = _endIndex = 0;

            return true;
        }

        if ( _firstIndex >= _endIndex && ! IsEmpty )
        {
            Assume.IsGreaterThan( Count, 1 );
            _items[_firstIndex++] = default!;
            if ( _firstIndex == _items.Length )
                _firstIndex = 0;

            return true;
        }

        return false;
    }

    /// <summary>
    /// Attempts to remove and return the first item from this queue.
    /// </summary>
    /// <param name="item"><b>out</b> parameter that returns the removed first item.</param>
    /// <returns><b>true</b> when queue was not empty and first item was removed, otherwise <b>false</b>.</returns>
    public bool TryDequeue([MaybeNullWhen( false )] out T item)
    {
        if ( _firstIndex < _endIndex )
        {
            item = _items[_firstIndex];
            _items[_firstIndex++] = default!;
            if ( _firstIndex == _endIndex )
                _firstIndex = _endIndex = 0;

            return true;
        }

        if ( _firstIndex >= _endIndex && ! IsEmpty )
        {
            Assume.IsGreaterThan( Count, 1 );
            item = _items[_firstIndex];
            _items[_firstIndex++] = default!;
            if ( _firstIndex == _items.Length )
                _firstIndex = 0;

            return true;
        }

        item = default;
        return false;
    }

    /// <summary>
    /// Attempts to remove a range of items from the start of this queue.
    /// </summary>
    /// <param name="count">Number of elements to remove.</param>
    /// <returns>Actual number of removed elements.</returns>
    /// <remarks>
    /// Does nothing, when <paramref name="count"/> is less than or equal to <b>0</b>.
    /// When <paramref name="count"/> is greater than queue's <see cref="Count"/>,
    /// then clears the queue and returns its old <see cref="Count"/>.
    /// </remarks>
    public int DequeueRange(int count)
    {
        if ( count <= 0 )
            return 0;

        if ( _firstIndex < _endIndex )
        {
            var maxCount = _endIndex - _firstIndex;
            if ( count >= maxCount )
            {
                _items.AsSpan( _firstIndex, maxCount ).Clear();
                _firstIndex = _endIndex = 0;
                return maxCount;
            }

            _items.AsSpan( _firstIndex, count ).Clear();
            _firstIndex += count;
            return count;
        }

        if ( _firstIndex >= _endIndex && ! IsEmpty )
        {
            var maxCount = _items.Length - _firstIndex;
            if ( count < maxCount )
            {
                _items.AsSpan( _firstIndex, count ).Clear();
                _firstIndex += count;
                return count;
            }

            _items.AsSpan( _firstIndex ).Clear();
            if ( count == maxCount )
            {
                _firstIndex = 0;
                return count;
            }

            count -= maxCount;
            var removed = maxCount;
            if ( count >= _endIndex )
            {
                removed += _endIndex;
                _items.AsSpan( 0, _endIndex ).Clear();
                _firstIndex = _endIndex = 0;
            }
            else
            {
                removed += count;
                _items.AsSpan( 0, count ).Clear();
                _firstIndex = count;
            }

            return removed;
        }

        return 0;
    }

    /// <summary>
    /// Removes all elements from this queue.
    /// </summary>
    public void Clear()
    {
        if ( _firstIndex < _endIndex )
            _items.AsSpan( _firstIndex, _endIndex - _firstIndex ).Clear();
        else if ( _firstIndex >= _endIndex && ! IsEmpty )
        {
            _items.AsSpan( _firstIndex ).Clear();
            _items.AsSpan( 0, _endIndex ).Clear();
        }

        _firstIndex = _endIndex = 0;
    }

    /// <summary>
    /// Attempts to increase or decrease this queue's <see cref="Capacity"/>, while ensuring that all current elements will fit.
    /// </summary>
    /// <param name="minCapacity">Minimum desired <see cref="Capacity"/> of this queue. Equal to <b>0</b> by default.</param>
    public void ResetCapacity(int minCapacity = 0)
    {
        if ( IsEmpty && minCapacity <= 0 )
        {
            _items = Array.Empty<T>();
            return;
        }

        var count = Count;
        if ( minCapacity < count )
            minCapacity = count;

        var capacity = GetCapacity( minCapacity );
        if ( capacity == _items.Length )
            return;

        var prevItems = _items;
        _items = new T[capacity];

        if ( _firstIndex < _endIndex )
            prevItems.AsSpan( _firstIndex, _endIndex - _firstIndex ).CopyTo( _items );
        else if ( _firstIndex >= _endIndex && ! IsEmpty )
        {
            var firstSlice = prevItems.AsSpan( _firstIndex );
            firstSlice.CopyTo( _items );
            prevItems.AsSpan( 0, _endIndex ).CopyTo( _items.AsSpan( firstSlice.Length ) );
        }

        _firstIndex = 0;
        _endIndex = count;
    }

    /// <summary>
    /// Creates a new <see cref="QueueSlimMemory{T}.Enumerator"/> instance for this queue.
    /// </summary>
    /// <returns>New <see cref="QueueSlimMemory{T}.Enumerator"/> instance.</returns>
    [Pure]
    public QueueSlimMemory<T>.Enumerator GetEnumerator()
    {
        return AsMemory().GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int GetWrappedIndex(int index)
    {
        var result = unchecked( ( uint )_firstIndex + ( uint )index );
        if ( result >= _items.Length )
            result = unchecked( result - ( uint )_items.Length );

        return unchecked( ( int )result );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static int GetCapacity(int minCapacity)
    {
        if ( minCapacity <= MinCapacity )
            minCapacity = MinCapacity;

        var result = BitOperations.RoundUpToPowerOf2( unchecked( ( uint )minCapacity ) );
        return result > int.MaxValue ? int.MaxValue : unchecked( ( int )result );
    }
}
