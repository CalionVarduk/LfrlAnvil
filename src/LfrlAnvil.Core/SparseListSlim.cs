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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Represents a slim version of a sparse dynamic array of objects.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct SparseListSlim<T>
{
    private LinkedEntry<T>[] _items;
    private NullableIndex _freeListHead;
    private NullableIndex _freeListTail;
    private NullableIndex _occupiedListHead;
    private NullableIndex _occupiedListTail;

    private SparseListSlim(int minCapacity)
    {
        _items = minCapacity > 0 ? new LinkedEntry<T>[Buffers.GetCapacity( minCapacity )] : Array.Empty<LinkedEntry<T>>();
        _freeListHead = _freeListTail = _occupiedListHead = _occupiedListTail = NullableIndex.Null;
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
    /// Gets a <see cref="Sequence"/> instance associated with this list, that allows to enumerate over all of its elements in index order.
    /// </summary>
    public Sequence Sequential => new Sequence( _items, Count );

    /// <summary>
    /// Gets the <see cref="LinkedListSlimNode{T}"/> that contains the oldest element in this list.
    /// </summary>
    public LinkedListSlimNode<T>? First => _occupiedListHead.HasValue ? new LinkedListSlimNode<T>( _items, _occupiedListHead.Value ) : null;

    /// <summary>
    /// Gets the <see cref="LinkedListSlimNode{T}"/> that contains the newest element in this list.
    /// </summary>
    public LinkedListSlimNode<T>? Last => _occupiedListTail.HasValue ? new LinkedListSlimNode<T>( _items, _occupiedListTail.Value ) : null;

    /// <summary>
    /// Gets a reference to an element at the given index, or null reference, when element at the given index does not exist.
    /// </summary>
    /// <param name="index">The zero-based index of the element reference to get.</param>
    public ref T this[int index]
    {
        get
        {
            if ( index < 0 || index >= _items.Length )
                return ref Unsafe.NullRef<T>();

            ref var entry = ref GetEntryRef( index );
            if ( ! entry.IsInOccupiedList )
                return ref Unsafe.NullRef<T>();

            return ref entry.Value;
        }
    }

    /// <summary>
    /// Creates a new empty <see cref="SparseListSlim{T}"/> instance.
    /// </summary>
    /// <param name="minCapacity">Minimum initial <see cref="Capacity"/> of the created list. Equal to <b>0</b> by default.</param>
    /// <returns>New <see cref="SparseListSlim{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SparseListSlim<T> Create(int minCapacity = 0)
    {
        return new SparseListSlim<T>( minCapacity );
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the tail of this list.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <returns>Zero-based index at which the added <paramref name="item"/> exists.</returns>
    public int Add(T item)
    {
        int result;
        if ( _freeListTail.HasValue )
        {
            Assume.IsLessThan( Count, _items.Length );
            Assume.True( _freeListHead.HasValue );

            ++Count;
            result = _freeListTail.Value;
            ref var entry = ref PopFreeListTail();
            entry.MakeOccupied( _occupiedListTail, NullableIndex.Null );
            entry.Value = item;
        }
        else
        {
            Assume.False( _freeListHead.HasValue );

            result = Count;
            if ( Count == _items.Length )
            {
                Count = checked( Count + 1 );
                var prevItems = _items;
                _items = new LinkedEntry<T>[Buffers.GetCapacity( Count )];
                prevItems.AsSpan().CopyTo( _items );
            }
            else
                ++Count;

            Assume.True( _items[result].IsUnused );
            ref var entry = ref GetEntryRef( result );
            entry.MakeOccupied( _occupiedListTail, NullableIndex.Null );
            entry.Value = item;
        }

        SetOccupiedListTail( result );
        return result;
    }

    /// <summary>
    /// Adds an element with default value to the tail of this list and returns its reference.
    /// </summary>
    /// <param name="index"><b>out</b> parameter that returns a zero-based index at which the added element exists.</param>
    /// <returns>Reference to the added element.</returns>
    public ref T? AddDefault(out int index)
    {
        index = Add( default! );
        ref var entry = ref GetEntryRef( index );
        return ref entry.Value!;
    }

    /// <summary>
    /// Attempts to add <paramref name="item"/> to this list at the specified position.
    /// </summary>
    /// <param name="index">Zero-based index at which to attempt to add the <paramref name="item"/>.</param>
    /// <param name="item">Item to add.</param>
    /// <returns>
    /// <b>true</b> when <paramref name="item"/> was added,
    /// otherwise <b>false</b>, when another item already exists at the specified position.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is less than zero.</exception>
    public bool TryAdd(int index, T item)
    {
        ref var entry = ref GetRefOrAddDefault( index, out var exists );
        if ( exists )
            return false;

        entry = item;
        return true;
    }

    /// <summary>
    /// Gets a reference to an element at the specified position, or adds a new element with default value and returns its reference,
    /// if element at the specified position does not exist.
    /// </summary>
    /// <param name="index">The zero-based index of the element reference to get, or to add a new element at.</param>
    /// <param name="exists">
    /// <b>out</b> parameter, that returns <b>true</b>, when an element already exists at the specified position,
    /// otherwise <b>false</b>, when a new element was added.
    /// </param>
    /// <returns>Reference to an element at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is less than zero.</exception>
    public ref T? GetRefOrAddDefault(int index, out bool exists)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0 );
        if ( index >= _items.Length )
        {
            var capacity = Buffers.GetCapacity( checked( index + 1 ) );
            var prevItems = _items;
            _items = new LinkedEntry<T>[capacity];
            prevItems.AsSpan().CopyTo( _items );
        }

        ref var entry = ref GetEntryRef( index );
        if ( entry.IsInOccupiedList )
        {
            exists = true;
            return ref entry.Value!;
        }

        exists = false;
        if ( entry.IsUnused )
            MovePrecedingUnusedEntriesToFreeList( index, ref entry );
        else
            RemoveFromFreeList( index, ref entry );

        ++Count;
        entry.MakeOccupied( _occupiedListTail, NullableIndex.Null );

        SetOccupiedListTail( index );
        return ref entry.Value!;
    }

    /// <summary>
    /// Attempts to remove an item at the specified position from this list.
    /// </summary>
    /// <param name="index">The zero-based index of an element to be removed.</param>
    /// <returns><b>true</b> when existing item was removed, otherwise <b>false</b>.</returns>
    public bool Remove(int index)
    {
        if ( index < 0 || index >= _items.Length )
            return false;

        ref var entry = ref GetEntryRef( index );
        if ( ! entry.IsInOccupiedList )
            return false;

        Remove( index, ref entry );
        return true;
    }

    /// <summary>
    /// Attempts to remove an item at the specified position from this list.
    /// </summary>
    /// <param name="index">The zero-based index of an element to be removed.</param>
    /// <param name="removed"><b>out</b> parameter, that returns removed item.</param>
    /// <returns><b>true</b> when existing item was removed, otherwise <b>false</b>.</returns>
    public bool Remove(int index, [MaybeNullWhen( false )] out T removed)
    {
        if ( index < 0 || index >= _items.Length )
        {
            removed = default;
            return false;
        }

        ref var entry = ref GetEntryRef( index );
        if ( ! entry.IsInOccupiedList )
        {
            removed = default;
            return false;
        }

        removed = entry.Value;
        Remove( index, ref entry );
        return true;
    }

    /// <summary>
    /// Checks whether or not an item at the specified position exists.
    /// </summary>
    /// <param name="index">Zero-based index to check for item existence.</param>
    /// <returns><b>true</b> when item at the specified position exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(int index)
    {
        if ( index < 0 || index >= _items.Length )
            return false;

        ref var entry = ref GetEntryRef( index );
        return entry.IsInOccupiedList;
    }

    /// <summary>
    /// Attempts to get a <see cref="LinkedListSlimNode{T}"/> that contains an item, that exists at the specified position.
    /// </summary>
    /// <param name="index">Zero-based index of an item to get a <see cref="LinkedListSlimNode{T}"/> for.</param>
    /// <returns>
    /// <see cref="LinkedListSlimNode{T}"/> instance that contains an item at the specified position,
    /// or <b>null</b>, when an item does not exist.
    /// </returns>
    [Pure]
    public LinkedListSlimNode<T>? GetNode(int index)
    {
        if ( index < 0 || index >= _items.Length )
            return null;

        ref var entry = ref GetEntryRef( index );
        return entry.IsInOccupiedList ? new LinkedListSlimNode<T>( _items, index ) : null;
    }

    /// <summary>
    /// Removes all elements from this list.
    /// </summary>
    public void Clear()
    {
        if ( ! _occupiedListTail.HasValue && ! _freeListTail.HasValue )
            return;

        _items.AsSpan().Clear();
        _occupiedListHead = _occupiedListTail = _freeListHead = _freeListTail = NullableIndex.Null;
        Count = 0;
    }

    /// <summary>
    /// Attempts to increase or decrease this list's <see cref="Capacity"/>, while ensuring that all current elements will fit.
    /// </summary>
    /// <param name="minCapacity">Minimum desired <see cref="Capacity"/> of this list. Equal to <b>0</b> by default.</param>
    public void ResetCapacity(int minCapacity = 0)
    {
        Buffers.ResetLinkedListCapacity( ref _items, ref _freeListHead, ref _freeListTail, _occupiedListHead, Count, minCapacity );
    }

    /// <summary>
    /// Creates a new <see cref="LinkedListSlimEnumerator{T}"/> instance for this list.
    /// </summary>
    /// <returns>New <see cref="LinkedListSlimEnumerator{T}"/> instance.</returns>
    /// <remarks>
    /// This allows to enumerate over <see cref="SparseListSlim{T}"/> instances in the order of item addition.
    /// See <see cref="Sequential"/> for the ability to enumerate in index order instead.
    /// </remarks>
    public LinkedListSlimEnumerator<T> GetEnumerator()
    {
        return new LinkedListSlimEnumerator<T>( _items, _occupiedListHead );
    }

    /// <summary>
    /// Represents an enumerable view of a <see cref="SparseListSlim{T}"/> instance,
    /// that allows to enumerate over all of its elements in index order.
    /// </summary>
    public readonly ref struct Sequence
    {
        private readonly ref LinkedEntry<T> _first;
        private readonly int _count;

        internal Sequence(LinkedEntry<T>[] items, int count)
        {
            _first = ref MemoryMarshal.GetArrayDataReference( items );
            _count = count;
        }

        /// <summary>
        /// Creates a new <see cref="Enumerator"/> instance for this sequence.
        /// </summary>
        /// <returns>New <see cref="Enumerator"/> instance.</returns>
        public Enumerator GetEnumerator()
        {
            return new Enumerator( this );
        }

        /// <summary>
        /// Lightweight enumerator implementation for <see cref="Sequence"/>.
        /// </summary>
        public ref struct Enumerator
        {
            private readonly ref LinkedEntry<T> _first;
            private int _remaining;
            private int _current;

            internal Enumerator(Sequence seq)
            {
                _first = ref seq._first;
                _remaining = seq._count;
                _current = -1;
            }

            /// <summary>
            /// Gets an element in the view, along with its index, at the current position of the enumerator.
            /// </summary>
            public KeyValuePair<int, T> Current
            {
                get
                {
                    ref var entry = ref GetEntryRef( _current );
                    return KeyValuePair.Create( _current, entry.Value );
                }
            }

            /// <summary>
            /// Advances the enumerator to the next element of the collection.
            /// </summary>
            /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            public bool MoveNext()
            {
                if ( _remaining <= 0 )
                    return false;

                --_remaining;
                ref var entry = ref Unsafe.Add( ref _first, ++_current );
                while ( ! entry.IsInOccupiedList )
                {
                    Assume.True( entry.IsInFreeList );
                    entry = ref Unsafe.Add( ref entry, 1 );
                    ++_current;
                }

                return true;
            }

            [Pure]
            [MethodImpl( MethodImplOptions.AggressiveInlining )]
            private ref LinkedEntry<T> GetEntryRef(int index)
            {
                return ref Unsafe.Add( ref _first, index );
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> GetEntryRef(int index)
    {
        Assume.IsInRange( index, 0, _items.Length - 1 );
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        return ref Unsafe.Add( ref first, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Remove(int index, ref LinkedEntry<T> entry)
    {
        RemoveFromOccupiedList( index, ref entry );
        entry.MakeFree( _freeListTail, NullableIndex.Null );
        entry.Value = default!;
        --Count;

        var i = NullableIndex.Create( index );
        if ( _freeListTail.HasValue )
        {
            Assume.True( _freeListHead.HasValue );
            ref var tail = ref GetFreeListTail();
            tail.MakeFree( tail.Prev, i );
            _freeListTail = i;
        }
        else
        {
            Assume.False( _freeListHead.HasValue );
            _freeListHead = _freeListTail = i;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void MovePrecedingUnusedEntriesToFreeList(int index, ref LinkedEntry<T> entry)
    {
        Assume.True( entry.IsUnused );
        if ( index <= 0 )
            return;

        ref var left = ref Unsafe.Subtract( ref entry, 1 );
        ref var tail = ref left;
        if ( ! left.IsUnused )
            return;

        --index;
        if ( _freeListTail.HasValue )
            tail = ref GetFreeListTail();
        else
        {
            Assume.False( _freeListHead.HasValue );
            left.MakeFree( NullableIndex.Null, NullableIndex.Null );
            tail = ref left;
            left = ref Unsafe.Subtract( ref left, 1 );
            _freeListHead = _freeListTail = NullableIndex.Create( index-- );
            if ( index < 0 || ! left.IsUnused )
                return;
        }

        do
        {
            left.MakeFree( _freeListTail, NullableIndex.Null );
            _freeListTail = NullableIndex.Create( index-- );
            tail.MakeFree( tail.Prev, _freeListTail );
            tail = ref left;
            left = ref Unsafe.Subtract( ref left, 1 );
        }
        while ( index >= 0 && left.IsUnused );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetOccupiedListTail(int index)
    {
        if ( ! _occupiedListTail.HasValue )
        {
            Assume.Equals( Count, 1 );
            Assume.False( _occupiedListHead.HasValue );
            _occupiedListHead = _occupiedListTail = NullableIndex.Create( index );
            return;
        }

        Assume.IsGreaterThan( Count, 1 );
        Assume.True( _occupiedListHead.HasValue );

        ref var tail = ref GetEntryRef( _occupiedListTail.Value );
        Assume.True( tail.IsInOccupiedList );
        Assume.Equals( tail.Next, NullableIndex.Null );
        _occupiedListTail = NullableIndex.Create( index );
        tail.MakeOccupied( tail.Prev, _occupiedListTail );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RemoveFromOccupiedList(int index, ref LinkedEntry<T> entry)
    {
        Assume.True( entry.IsInOccupiedList );
        Assume.True( _occupiedListHead.HasValue );
        Assume.True( _occupiedListTail.HasValue );

        var prev = entry.Prev;
        var next = entry.Next;

        if ( index == _occupiedListHead.Value )
        {
            if ( index == _occupiedListTail.Value )
            {
                Assume.Equals( Count, 1 );
                Assume.False( prev.HasValue );
                Assume.False( next.HasValue );
                _occupiedListHead = _occupiedListTail = NullableIndex.Null;
            }
            else
            {
                Assume.IsGreaterThan( Count, 1 );
                ref var nextEntry = ref GetEntryRef( next.Value );
                Assume.True( nextEntry.IsInOccupiedList );
                Assume.Equals( nextEntry.Prev.Value, index );
                nextEntry.MakeOccupied( NullableIndex.Null, nextEntry.Next );
                _occupiedListHead = next;
            }
        }
        else
        {
            Assume.IsGreaterThan( Count, 1 );
            if ( index == _occupiedListTail.Value )
            {
                ref var prevEntry = ref GetEntryRef( prev.Value );
                Assume.True( prevEntry.IsInOccupiedList );
                Assume.Equals( prevEntry.Next.Value, index );
                prevEntry.MakeOccupied( prevEntry.Prev, NullableIndex.Null );
                _occupiedListTail = prev;
            }
            else
            {
                ref var prevEntry = ref GetEntryRef( prev.Value );
                Assume.True( prevEntry.IsInOccupiedList );
                Assume.Equals( prevEntry.Next.Value, index );
                prevEntry.MakeOccupied( prevEntry.Prev, next );

                ref var nextEntry = ref GetEntryRef( next.Value );
                Assume.True( nextEntry.IsInOccupiedList );
                Assume.Equals( nextEntry.Prev.Value, index );
                nextEntry.MakeOccupied( prev, nextEntry.Next );
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> GetFreeListTail()
    {
        ref var entry = ref GetEntryRef( _freeListTail.Value );
        Assume.True( entry.IsInFreeList );
        Assume.False( entry.Next.HasValue );
        return ref entry;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref LinkedEntry<T> PopFreeListTail()
    {
        ref var tail = ref GetFreeListTail();
        var prev = tail.Prev;

        if ( prev.HasValue )
        {
            Assume.NotEquals( _freeListHead, _freeListTail );
            ref var entry = ref GetEntryRef( prev.Value );
            Assume.True( entry.IsInFreeList );
            Assume.Equals( entry.Next, _freeListTail );
            entry.MakeFree( entry.Prev, NullableIndex.Null );
            _freeListTail = prev;
        }
        else
        {
            Assume.Equals( _freeListHead, _freeListTail );
            _freeListTail = _freeListHead = NullableIndex.Null;
        }

        return ref tail;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RemoveFromFreeList(int index, ref LinkedEntry<T> entry)
    {
        Assume.True( entry.IsInFreeList );
        Assume.True( _freeListHead.HasValue );
        Assume.True( _freeListTail.HasValue );

        var prev = entry.Prev;
        var next = entry.Next;

        if ( index == _freeListHead.Value )
        {
            if ( index == _freeListTail.Value )
            {
                Assume.False( prev.HasValue );
                Assume.False( next.HasValue );
                _freeListHead = _freeListTail = NullableIndex.Null;
            }
            else
            {
                ref var nextEntry = ref GetEntryRef( next.Value );
                Assume.True( nextEntry.IsInFreeList );
                Assume.Equals( nextEntry.Prev.Value, index );
                nextEntry.MakeFree( NullableIndex.Null, nextEntry.Next );
                _freeListHead = next;
            }
        }
        else
        {
            if ( index == _freeListTail.Value )
            {
                ref var prevEntry = ref GetEntryRef( prev.Value );
                Assume.True( prevEntry.IsInFreeList );
                Assume.Equals( prevEntry.Next.Value, index );
                prevEntry.MakeFree( prevEntry.Prev, NullableIndex.Null );
                _freeListTail = prev;
            }
            else
            {
                ref var prevEntry = ref GetEntryRef( prev.Value );
                Assume.True( prevEntry.IsInFreeList );
                Assume.Equals( prevEntry.Next.Value, index );
                prevEntry.MakeFree( prevEntry.Prev, next );

                ref var nextEntry = ref GetEntryRef( next.Value );
                Assume.True( nextEntry.IsInFreeList );
                Assume.Equals( nextEntry.Prev.Value, index );
                nextEntry.MakeFree( prev, nextEntry.Next );
            }
        }
    }
}
