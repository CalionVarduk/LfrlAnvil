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
using LfrlAnvil.Extensions;

namespace LfrlAnvil;

/// <summary>
/// Represents a slim version of last-in, first-out collection of objects.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct StackSlim<T>
{
    /// <summary>
    /// Minimum capacity for non-empty stacks. Equal to <b>4</b>.
    /// </summary>
    public const int MinCapacity = 1 << 2;

    private T[] _items;

    private StackSlim(int minCapacity)
    {
        _items = minCapacity <= 0 ? Array.Empty<T>() : new T[GetCapacity( minCapacity )];
        Count = 0;
    }

    /// <summary>
    /// Gets the number of elements contained in this stack.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the number of maximum elements that this stack can contain without resizing the underlying buffer.
    /// </summary>
    public int Capacity => _items.Length;

    /// <summary>
    /// Specifies whether or not this stack is empty.
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
            return ref Unsafe.Add( ref first, _items.Length - Count + index );
        }
    }

    /// <summary>
    /// Creates a new empty <see cref="StackSlim{T}"/> instance.
    /// </summary>
    /// <param name="minCapacity">Minimum initial <see cref="Capacity"/> of the created stack. Equal to <b>0</b> by default.</param>
    /// <returns>New <see cref="StackSlim{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static StackSlim<T> Create(int minCapacity = 0)
    {
        return new StackSlim<T>( minCapacity );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlyMemory{T}"/> instance that represents a view of this stack.
    /// </summary>
    /// <returns>New <see cref="ReadOnlyMemory{T}"/> instance.</returns>
    [Pure]
    public ReadOnlyMemory<T> AsMemory()
    {
        return _items.AsMemory( _items.Length - Count );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlySpan{T}"/> instance that represents a view of this stack.
    /// </summary>
    /// <returns>New <see cref="ReadOnlySpan{T}"/> instance.</returns>
    [Pure]
    public ReadOnlySpan<T> AsSpan()
    {
        return _items.AsSpan( _items.Length - Count );
    }

    /// <summary>
    /// Returns a reference to the element at the top of this stack.
    /// </summary>
    /// <returns>Reference to the top element.</returns>
    /// <remarks>May return an invalid reference, when this stack is empty.</remarks>
    [Pure]
    public ref T Top()
    {
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        return ref Unsafe.Add( ref first, _items.Length - Count );
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the top of this stack.
    /// </summary>
    /// <param name="item">Item to add.</param>
    public void Push(T item)
    {
        var nextCount = checked( Count + 1 );
        if ( Count == _items.Length )
        {
            var prevItems = _items;
            _items = new T[GetCapacity( nextCount )];
            prevItems.AsSpan().CopyTo( _items.AsSpan( _items.Length - Count ) );
        }

        Count = nextCount;
        _items[^Count] = item;
    }

    /// <summary>
    /// Adds a range of <paramref name="items"/> to the top of this stack.
    /// </summary>
    /// <param name="items">Items to add.</param>
    /// <remarks>Does nothing when <paramref name="items"/> are empty.</remarks>
    public void PushRange(ReadOnlySpan<T> items)
    {
        if ( items.Length == 0 )
            return;

        var nextCount = checked( Count + items.Length );
        if ( _items.Length < nextCount )
        {
            var prevItems = _items;
            _items = new T[GetCapacity( nextCount )];
            prevItems.AsSpan( prevItems.Length - Count ).CopyTo( _items.AsSpan( _items.Length - Count ) );
        }

        ref var item = ref MemoryMarshal.GetArrayDataReference( _items )!;
        item = ref Unsafe.Add( ref item, _items.Length - Count - 1 )!;

        foreach ( var toAdd in items )
        {
            item = toAdd!;
            item = ref Unsafe.Subtract( ref item, 1 )!;
        }

        Count = nextCount;
    }

    /// <summary>
    /// Attempts to remove the top item from this stack.
    /// </summary>
    /// <returns><b>true</b> when stack was not empty and top item was removed, otherwise <b>false</b>.</returns>
    public bool Pop()
    {
        if ( Count == 0 )
            return false;

        _items[^Count--] = default!;
        return true;
    }

    /// <summary>
    /// Attempts to remove and return the top item from this stack.
    /// </summary>
    /// <param name="item"><b>out</b> parameter that returns the removed top item.</param>
    /// <returns><b>true</b> when stack was not empty and top item was removed, otherwise <b>false</b>.</returns>
    public bool TryPop([MaybeNullWhen( false )] out T item)
    {
        if ( Count == 0 )
        {
            item = default;
            return false;
        }

        var index = _items.Length - Count--;
        item = _items[index];
        _items[index] = default!;
        return true;
    }

    /// <summary>
    /// Attempts to remove a range of top items from this stack.
    /// </summary>
    /// <param name="count">Number of elements to remove.</param>
    /// <returns>Actual number of removed elements.</returns>
    /// <remarks>
    /// Does nothing, when <paramref name="count"/> is less than or equal to <b>0</b>.
    /// When <paramref name="count"/> is greater than stack's <see cref="Count"/>,
    /// then clears the stack and returns its old <see cref="Count"/>.
    /// </remarks>
    public int PopRange(int count)
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
            _items.AsSpan( _items.Length - Count, count ).Clear();
            Count -= count;
        }

        return count;
    }

    /// <summary>
    /// Removes all elements from this stack.
    /// </summary>
    public void Clear()
    {
        _items.AsSpan( _items.Length - Count ).Clear();
        Count = 0;
    }

    /// <summary>
    /// Attempts to increase or decrease this stack's <see cref="Capacity"/>, while ensuring that all current elements will fit.
    /// </summary>
    /// <param name="minCapacity">Minimum desired <see cref="Capacity"/> of this stack. Equal to <b>0</b> by default.</param>
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
        prevItems.AsSpan( prevItems.Length - Count ).CopyTo( _items.AsSpan( _items.Length - Count ) );
    }

    /// <summary>
    /// Creates a new <see cref="ReadOnlySpan{T}.Enumerator"/> instance for this stack.
    /// </summary>
    /// <returns>New <see cref="ReadOnlySpan{T}.Enumerator"/> instance.</returns>
    [Pure]
    public ReadOnlySpan<T>.Enumerator GetEnumerator()
    {
        return AsMemory().GetEnumerator();
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
