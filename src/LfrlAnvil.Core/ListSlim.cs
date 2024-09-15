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
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace LfrlAnvil;

/// <summary>
/// Represents a slim version of a dynamic array of objects.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct ListSlim<T>
{
    /// <summary>
    /// Minimum capacity for non-empty lists. Equal to <b>4</b>.
    /// </summary>
    public const int MinCapacity = 1 << 2;

    private T[] _items;

    private ListSlim(int minCapacity)
    {
        _items = minCapacity <= 0 ? Array.Empty<T>() : new T[GetCapacity( minCapacity )];
        Count = 0;
    }

    /// <summary>
    /// Gets the number of elements contained in this list.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the number of maximum elements that this list can contain without resizing the underlying buffer.
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// Specifies whether or not this list is empty.
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Gets a reference to an element at the given index.
    /// </summary>
    /// <param name="index">The zero-based index of the element reference to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public ref T this[int index]
    {
        get
        {
            Ensure.IsInIndexRange( index, Count );
            ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
            return ref Unsafe.Add( ref first, index );
        }
    }

    /// <summary>
    /// Creates a new empty <see cref="ListSlim{T}"/> instance.
    /// </summary>
    /// <param name="minCapacity">Minimum initial <see cref="Capacity"/> of the created list. Equal to <b>0</b> by default.</param>
    /// <returns>New <see cref="ListSlim{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ListSlim<T> Create(int minCapacity = 0)
    {
        return new ListSlim<T>( minCapacity );
    }

    /// <summary>
    /// Creates a new <see cref="Memory{T}"/> instance that represents a view of this list.
    /// </summary>
    /// <returns>New <see cref="Memory{T}"/> instance.</returns>
    [Pure]
    public Memory<T> AsMemory()
    {
        return _items.AsMemory( 0, Count );
    }

    /// <summary>
    /// Creates a new <see cref="Span{T}"/> instance that represents a view of this list.
    /// </summary>
    /// <returns>New <see cref="Span{T}"/> instance.</returns>
    [Pure]
    public Span<T> AsSpan()
    {
        return _items.AsSpan( 0, Count );
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the end of this list.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void Add(T item)
    {
        var nextCount = checked( Count + 1 );
        if ( Count == _items.Length )
        {
            var prevItems = _items;
            _items = new T[GetCapacity( nextCount )];
            prevItems.AsSpan().CopyTo( _items );
        }

        _items[Count] = item;
        Count = nextCount;
    }

    /// <summary>
    /// Adds a range of <paramref name="items"/> to the end of this list.
    /// </summary>
    /// <param name="items">Items to add.</param>
    /// <remarks>Does nothing when <paramref name="items"/> are empty.</remarks>
    public void AddRange(ReadOnlySpan<T> items)
    {
        if ( items.Length == 0 )
            return;

        var nextCount = checked( Count + items.Length );
        if ( _items.Length < nextCount )
        {
            var prevItems = _items;
            _items = new T[GetCapacity( nextCount )];
            prevItems.AsSpan( 0, Count ).CopyTo( _items );
        }

        items.CopyTo( _items.AsSpan( Count ) );
        Count = nextCount;
    }

    /// <summary>
    /// Adds <paramref name="item"/> at the specified position to this list.
    /// </summary>
    /// <param name="index">The zero-based index at which to add the <paramref name="item"/>.</param>
    /// <param name="item">Item to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public void InsertAt(int index, T item)
    {
        Ensure.IsInRange( index, 0, Count );

        var nextCount = checked( Count + 1 );
        if ( Count == _items.Length )
        {
            var prevItems = _items;
            _items = new T[GetCapacity( nextCount )];
            prevItems.AsSpan( 0, index ).CopyTo( _items );
            prevItems.AsSpan( index ).CopyTo( _items.AsSpan( index + 1 ) );
        }
        else
            _items.AsSpan( index, Count - index ).CopyTo( _items.AsSpan( index + 1 ) );

        _items[index] = item;
        Count = nextCount;
    }

    /// <summary>
    /// Adds a range of <paramref name="items"/> at the specified position to this list.
    /// </summary>
    /// <param name="index">The zero-based index at which to start adding <paramref name="items"/>.</param>
    /// <param name="items">Items to add.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    /// <remarks>Does nothing when <paramref name="items"/> are empty.</remarks>
    public void InsertRangeAt(int index, ReadOnlySpan<T> items)
    {
        Ensure.IsInRange( index, 0, Count );
        if ( items.Length == 0 )
            return;

        var nextCount = checked( Count + items.Length );
        if ( _items.Length < nextCount )
        {
            var prevItems = _items;
            _items = new T[GetCapacity( nextCount )];
            prevItems.AsSpan( 0, index ).CopyTo( _items );
            prevItems.AsSpan( index ).CopyTo( _items.AsSpan( index + items.Length ) );
        }
        else
            _items.AsSpan( index, Count - index ).CopyTo( _items.AsSpan( index + items.Length ) );

        items.CopyTo( _items.AsSpan( index ) );
        Count = nextCount;
    }

    /// <summary>
    /// Attempts to remove the last item from this list.
    /// </summary>
    /// <returns><b>true</b> when list was not empty and last item was removed, otherwise <b>false</b>.</returns>
    public bool RemoveLast()
    {
        if ( Count == 0 )
            return false;

        _items[--Count] = default!;
        return true;
    }

    /// <summary>
    /// Attempts to remove a range of last items from this list.
    /// </summary>
    /// <param name="count">Number of elements to remove.</param>
    /// <returns>Actual number of removed elements.</returns>
    /// <remarks>
    /// Does nothing, when <paramref name="count"/> is less than or equal to <b>0</b>.
    /// When <paramref name="count"/> is greater than list's <see cref="Count"/>,
    /// then clears the list and returns its old <see cref="Count"/>.
    /// </remarks>
    public int RemoveLastRange(int count)
    {
        if ( count <= 0 )
            return 0;

        if ( count >= Count )
        {
            count = Count;
            Clear();
        }
        else
        {
            _items.AsSpan( Count - count, count ).Clear();
            Count -= count;
        }

        return count;
    }

    /// <summary>
    /// Removes an item at the specified position from this list.
    /// </summary>
    /// <param name="index">The zero-based index of an element to be removed.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public void RemoveAt(int index)
    {
        Ensure.IsInIndexRange( index, Count );
        var nextIndex = index + 1;
        _items.AsSpan( nextIndex, Count - nextIndex ).CopyTo( _items.AsSpan( index ) );
        _items[--Count] = default!;
    }

    /// <summary>
    /// Removes a range of items from this list, starting at the specified position.
    /// </summary>
    /// <param name="index">The zero-based index of the first element to be removed.</param>
    /// <param name="count">Number of elements to remove.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> or <paramref name="count"/> are out of bounds.
    /// </exception>
    /// <remarks>Does nothing, when <paramref name="count"/> is less than or equal to <b>0</b>.</remarks>
    public void RemoveRangeAt(int index, int count)
    {
        Ensure.IsInIndexRange( index, Count );
        if ( count <= 0 )
            return;

        var nextIndex = index + count;
        _items.AsSpan( nextIndex, Count - nextIndex ).CopyTo( _items.AsSpan( index ) );
        Count -= count;
        _items.AsSpan( Count, count ).Clear();
    }

    /// <summary>
    /// Removes all elements from this list.
    /// </summary>
    public void Clear()
    {
        _items.AsSpan( 0, Count ).Clear();
        Count = 0;
    }

    /// <summary>
    /// Attempts to increase or decrease this list's <see cref="Capacity"/>, while ensuring that all current elements will fit.
    /// </summary>
    /// <param name="minCapacity">Minimum desired <see cref="Capacity"/> of this list. Equal to <b>0</b> by default.</param>
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
        prevItems.AsSpan( 0, Count ).CopyTo( _items );
    }

    /// <summary>
    /// Creates a new <see cref="Span{T}.Enumerator"/> instance for this list.
    /// </summary>
    /// <returns>New <see cref="Span{T}.Enumerator"/> instance.</returns>
    [Pure]
    public Span<T>.Enumerator GetEnumerator()
    {
        return AsSpan().GetEnumerator();
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
