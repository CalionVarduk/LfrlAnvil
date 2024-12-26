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
/// Represents a slim version of a linked list of objects.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct LinkedListSlim<T>
{
    /// <summary>
    /// Represents a single element node that belongs to a <see cref="LinkedListSlim{T}"/> instance.
    /// </summary>
    public readonly struct Node
    {
        private readonly Entry[] _items;

        internal Node(Entry[] items, int index)
        {
            _items = items;
            Index = index;
        }

        /// <summary>
        /// Specifies the zero-based index at which this node can be found in its parent list.
        /// </summary>
        public int Index { get; }

        /// <summary>
        /// Gets a reference to this node's element.
        /// </summary>
        public ref T Value
        {
            get
            {
                ref var entry = ref GetEntryRef();
                return ref entry.Value;
            }
        }

        /// <summary>
        /// Gets a predecessor node or null, if this node is the first node in its parent list.
        /// </summary>
        public Node? Prev
        {
            get
            {
                ref var entry = ref GetEntryRef();
                var index = entry.Prev;
                return index.HasValue ? new Node( _items, index.Value ) : null;
            }
        }

        /// <summary>
        /// Gets a successor node or null, if this node is the last node in its parent list.
        /// </summary>
        public Node? Next
        {
            get
            {
                ref var entry = ref GetEntryRef();
                var index = entry.Next;
                return index.HasValue ? new Node( _items, index.Value ) : null;
            }
        }

        /// <inheritdoc />
        [Pure]
        public override string ToString()
        {
            return $"[{Index}]: {Value}";
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ref Entry GetEntryRef()
        {
            ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
            ref var entry = ref Unsafe.Add( ref first, Index );
            Assume.True( entry.IsInOccupiedList );
            return ref entry;
        }
    }

    internal struct Entry
    {
        private const ulong TypeMask = 3UL << 62;
        private const ulong FreeListMarker = 2UL << 62;
        private const ulong OccupiedListMarker = 1UL << 62;

        private ulong _flags;

        internal T Value;
        internal bool IsUnused => _flags == 0;
        internal bool IsInFreeList => (_flags & TypeMask) == FreeListMarker;
        internal bool IsInOccupiedList => (_flags & TypeMask) == OccupiedListMarker;
        internal NullableIndex Prev => NullableIndex.CreateUnsafe( unchecked( ( int )(_flags >> 31) & NullableIndex.NullValue ) );
        internal NullableIndex Next => NullableIndex.CreateUnsafe( unchecked( ( int )_flags & NullableIndex.NullValue ) );

        [Pure]
        public override string ToString()
        {
            return IsUnused
                ? "(unused)"
                : IsInFreeList
                    ? $"(free) Prev: {Prev}, Next: {Next}"
                    : $"Value: {Value}, Prev: {Prev}, Next: {Next}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void MakeOccupied(NullableIndex prev, NullableIndex next)
        {
            Assume.IsGreaterThanOrEqualTo( prev.Value, 0 );
            Assume.IsGreaterThanOrEqualTo( next.Value, 0 );
            _flags = unchecked( ( uint )next.Value | (( ulong )prev.Value << 31) | OccupiedListMarker );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void MakeFree(NullableIndex prev, NullableIndex next)
        {
            Assume.IsGreaterThanOrEqualTo( prev.Value, 0 );
            Assume.IsGreaterThanOrEqualTo( next.Value, 0 );
            _flags = unchecked( ( uint )next.Value | (( ulong )prev.Value << 31) | FreeListMarker );
        }
    }

    private Entry[] _items;
    private NullableIndex _freeListHead;
    private NullableIndex _freeListTail;
    private NullableIndex _occupiedListHead;
    private NullableIndex _occupiedListTail;

    private LinkedListSlim(int minCapacity)
    {
        _items = minCapacity <= 0 ? Array.Empty<Entry>() : new Entry[Buffers.GetCapacity( minCapacity )];
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
    /// Gets the <see cref="Node"/> that contains the first element in this list.
    /// </summary>
    public Node? First => _occupiedListHead.HasValue ? new Node( _items, _occupiedListHead.Value ) : null;

    /// <summary>
    /// Gets the <see cref="Node"/> that contains the last element in this list.
    /// </summary>
    public Node? Last => _occupiedListTail.HasValue ? new Node( _items, _occupiedListTail.Value ) : null;

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
    /// Creates a new empty <see cref="LinkedListSlim{T}"/> instance.
    /// </summary>
    /// <param name="minCapacity">Minimum initial <see cref="Capacity"/> of the created list. Equal to <b>0</b> by default.</param>
    /// <returns>New <see cref="LinkedListSlim{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static LinkedListSlim<T> Create(int minCapacity = 0)
    {
        return new LinkedListSlim<T>( minCapacity );
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the start of this list.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <returns>Index of the node that holds added <paramref name="item"/>.</returns>
    public int AddFirst(T item)
    {
        ref var entry = ref AddDefaultEntryRef( out var index );
        entry.Value = item;

        if ( _occupiedListHead.HasValue )
        {
            Assume.True( _occupiedListTail.HasValue );
            entry.MakeOccupied( NullableIndex.Null, _occupiedListHead );
            ref var head = ref GetEntryRef( _occupiedListHead.Value );
            head.MakeOccupied( NullableIndex.Create( index ), head.Next );
            _occupiedListHead = NullableIndex.Create( index );
        }
        else
        {
            Assume.False( _occupiedListTail.HasValue );
            entry.MakeOccupied( NullableIndex.Null, NullableIndex.Null );
            _occupiedListHead = _occupiedListTail = NullableIndex.Create( index );
        }

        return index;
    }

    /// <summary>
    /// Adds <paramref name="item"/> to the end of this list.
    /// </summary>
    /// <param name="item">Item to add.</param>
    /// <returns>Index of the node that holds added <paramref name="item"/>.</returns>
    public int AddLast(T item)
    {
        ref var entry = ref AddDefaultEntryRef( out var index );
        entry.Value = item;

        if ( _occupiedListTail.HasValue )
        {
            Assume.True( _occupiedListHead.HasValue );
            entry.MakeOccupied( _occupiedListTail, NullableIndex.Null );
            ref var tail = ref GetEntryRef( _occupiedListTail.Value );
            tail.MakeOccupied( tail.Prev, NullableIndex.Create( index ) );
            _occupiedListTail = NullableIndex.Create( index );
        }
        else
        {
            Assume.False( _occupiedListHead.HasValue );
            entry.MakeOccupied( NullableIndex.Null, NullableIndex.Null );
            _occupiedListHead = _occupiedListTail = NullableIndex.Create( index );
        }

        return index;
    }

    /// <summary>
    /// Adds <paramref name="item"/> directly before a node identified by the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Identifier of the node before which to add the new <paramref name="item"/>.</param>
    /// <param name="item">Item to add.</param>
    /// <returns>
    /// Index of the node that holds added <paramref name="item"/> or <b>-1</b>,
    /// when provided <paramref name="index"/> does not point to a valid node.
    /// </returns>
    public int AddBefore(int index, T item)
    {
        if ( index == _occupiedListHead.Value && _occupiedListHead.HasValue )
            return AddFirst( item );

        if ( ! Contains( index ) )
            return -1;

        ref var entry = ref AddDefaultEntryRef( out var result );
        ref var next = ref GetEntryRef( index );
        var prevIndex = next.Prev;
        Assume.True( prevIndex.HasValue );
        ref var prev = ref GetEntryRef( prevIndex.Value );

        entry.Value = item;
        entry.MakeOccupied( prevIndex, NullableIndex.Create( index ) );
        next.MakeOccupied( NullableIndex.Create( result ), next.Next );
        prev.MakeOccupied( prev.Prev, NullableIndex.Create( result ) );
        return result;
    }

    /// <summary>
    /// Adds <paramref name="item"/> directly after a node identified by the provided <paramref name="index"/>.
    /// </summary>
    /// <param name="index">Identifier of the node after which to add the new <paramref name="item"/>.</param>
    /// <param name="item">Item to add.</param>
    /// <returns>
    /// Index of the node that holds added <paramref name="item"/> or <b>-1</b>,
    /// when provided <paramref name="index"/> does not point to a valid node.
    /// </returns>
    public int AddAfter(int index, T item)
    {
        if ( index == _occupiedListTail.Value && _occupiedListTail.HasValue )
            return AddLast( item );

        if ( ! Contains( index ) )
            return -1;

        ref var entry = ref AddDefaultEntryRef( out var result );
        ref var prev = ref GetEntryRef( index );
        var nextIndex = prev.Next;
        Assume.True( nextIndex.HasValue );
        ref var next = ref GetEntryRef( nextIndex.Value );

        entry.Value = item;
        entry.MakeOccupied( NullableIndex.Create( index ), nextIndex );
        prev.MakeOccupied( prev.Prev, NullableIndex.Create( result ) );
        next.MakeOccupied( NullableIndex.Create( result ), next.Next );
        return result;
    }

    /// <summary>
    /// Removes the <see cref="First"/> node from this list.
    /// </summary>
    /// <returns><b>true</b> when list is not empty and <see cref="First"/> node was removed, otherwise <b>false</b>.</returns>
    public bool RemoveFirst()
    {
        if ( ! _occupiedListHead.HasValue )
            return false;

        var index = _occupiedListHead.Value;
        Assume.True( _occupiedListTail.HasValue );
        ref var head = ref GetEntryRef( index );
        UnlinkFirstNode( ref head );
        RemoveEntry( index, ref head );
        return true;
    }

    /// <summary>
    /// Removes the <see cref="First"/> node from this list.
    /// </summary>
    /// <param name="removed"><b>out</b> parameter, that returns removed item.</param>
    /// <returns><b>true</b> when list is not empty and <see cref="First"/> node was removed, otherwise <b>false</b>.</returns>
    public bool RemoveFirst([MaybeNullWhen( false )] out T removed)
    {
        if ( ! _occupiedListHead.HasValue )
        {
            removed = default;
            return false;
        }

        var index = _occupiedListHead.Value;
        Assume.True( _occupiedListTail.HasValue );
        ref var head = ref GetEntryRef( index );
        UnlinkFirstNode( ref head );
        removed = head.Value;
        RemoveEntry( index, ref head );
        return true;
    }

    /// <summary>
    /// Removes the <see cref="Last"/> node from this list.
    /// </summary>
    /// <returns><b>true</b> when list is not empty and <see cref="Last"/> node was removed, otherwise <b>false</b>.</returns>
    public bool RemoveLast()
    {
        if ( ! _occupiedListTail.HasValue )
            return false;

        var index = _occupiedListTail.Value;
        Assume.True( _occupiedListHead.HasValue );
        ref var tail = ref GetEntryRef( index );
        UnlinkLastNode( ref tail );
        RemoveEntry( index, ref tail );
        return true;
    }

    /// <summary>
    /// Removes the <see cref="Last"/> node from this list.
    /// </summary>
    /// <param name="removed"><b>out</b> parameter, that returns removed item.</param>
    /// <returns><b>true</b> when list is not empty and <see cref="Last"/> node was removed, otherwise <b>false</b>.</returns>
    public bool RemoveLast([MaybeNullWhen( false )] out T removed)
    {
        if ( ! _occupiedListTail.HasValue )
        {
            removed = default;
            return false;
        }

        var index = _occupiedListTail.Value;
        Assume.True( _occupiedListHead.HasValue );
        ref var tail = ref GetEntryRef( index );
        UnlinkLastNode( ref tail );
        removed = tail.Value;
        RemoveEntry( index, ref tail );
        return true;
    }

    /// <summary>
    /// Removes a node identified by the provided <paramref name="index"/> from this list.
    /// </summary>
    /// <param name="index">Index of a node to remove.</param>
    /// <returns><b>true</b> when node was removed successfully, otherwise <b>false</b>.</returns>
    public bool Remove(int index)
    {
        if ( index == _occupiedListHead.Value )
            return RemoveFirst();

        if ( index == _occupiedListTail.Value )
            return RemoveLast();

        if ( index < 0 || index >= _items.Length )
            return false;

        ref var entry = ref GetEntryRef( index );
        if ( ! entry.IsInOccupiedList )
            return false;

        ref var prev = ref GetEntryRef( entry.Prev.Value );
        ref var next = ref GetEntryRef( entry.Next.Value );
        prev.MakeOccupied( prev.Prev, entry.Next );
        next.MakeOccupied( entry.Prev, next.Next );
        RemoveEntry( index, ref entry );
        return true;
    }

    /// <summary>
    /// Removes a node identified by the provided <paramref name="index"/> from this list.
    /// </summary>
    /// <param name="index">Index of a node to remove.</param>
    /// <param name="removed"><b>out</b> parameter, that returns removed item.</param>
    /// <returns><b>true</b> when node was removed successfully, otherwise <b>false</b>.</returns>
    public bool Remove(int index, [MaybeNullWhen( false )] out T removed)
    {
        if ( index == _occupiedListHead.Value )
            return RemoveFirst( out removed );

        if ( index == _occupiedListTail.Value )
            return RemoveLast( out removed );

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

        ref var prev = ref GetEntryRef( entry.Prev.Value );
        ref var next = ref GetEntryRef( entry.Next.Value );
        prev.MakeOccupied( prev.Prev, entry.Next );
        next.MakeOccupied( entry.Prev, next.Next );
        removed = entry.Value;
        RemoveEntry( index, ref entry );
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
    /// Attempts to get a <see cref="Node"/> that contains an item, that exists at the specified position.
    /// </summary>
    /// <param name="index">Zero-based index of an item to get a <see cref="Node"/> for.</param>
    /// <returns>
    /// <see cref="Node"/> instance that contains an item at the specified position, or <b>null</b>, when an item does not exist.
    /// </returns>
    [Pure]
    public Node? GetNode(int index)
    {
        if ( index < 0 || index >= _items.Length )
            return null;

        ref var entry = ref GetEntryRef( index );
        return entry.IsInOccupiedList ? new Node( _items, index ) : null;
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
        if ( IsEmpty && minCapacity <= 0 )
        {
            _freeListHead = _freeListTail = NullableIndex.Null;
            _items = Array.Empty<Entry>();
            return;
        }

        var count = Count;
        if ( minCapacity < count )
            minCapacity = count;

        var capacity = Buffers.GetCapacity( minCapacity );
        if ( capacity == _items.Length )
            return;

        var prevItems = _items;
        if ( capacity > _items.Length )
        {
            _items = new Entry[capacity];
            prevItems.AsSpan().CopyTo( _items );
            return;
        }

        if ( ! _freeListTail.HasValue )
        {
            Assume.False( _freeListHead.HasValue );
            _items = new Entry[capacity];
            prevItems.AsSpan( 0, Count ).CopyTo( _items );
            return;
        }

        var endIndex = FindMaxOccupiedIndex() + 1;
        if ( endIndex > minCapacity )
        {
            capacity = Buffers.GetCapacity( endIndex );
            if ( capacity == _items.Length )
                return;
        }

        Assume.IsGreaterThanOrEqualTo( endIndex, Count );
        Assume.IsLessThan( capacity, _items.Length );
        _items = new Entry[capacity];
        prevItems.AsSpan( 0, endIndex ).CopyTo( _items );
        RebuildFreeList( endIndex );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this list.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _items, _occupiedListHead );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="LinkedListSlim{T}"/>.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly ref Entry _first;
        private NullableIndex _current;
        private NullableIndex _next;

        internal Enumerator(Entry[] items, NullableIndex head)
        {
            _first = ref MemoryMarshal.GetArrayDataReference( items );
            _current = NullableIndex.Null;
            _next = head;
        }

        /// <summary>
        /// Gets an element in the view, along with its index, at the current position of the enumerator.
        /// </summary>
        public KeyValuePair<int, T> Current
        {
            get
            {
                ref var entry = ref GetEntryRef( _current );
                return KeyValuePair.Create( _current.Value, entry.Value );
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            if ( ! _next.HasValue )
            {
                _current = NullableIndex.Null;
                return false;
            }

            ref var entry = ref GetEntryRef( _next );
            _current = _next;
            _next = entry.Next;
            return true;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ref Entry GetEntryRef(NullableIndex index)
        {
            return ref Unsafe.Add( ref _first, index.Value );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref Entry GetEntryRef(int index)
    {
        Assume.IsInRange( index, 0, _items.Length - 1 );
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        return ref Unsafe.Add( ref first, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref Entry AddDefaultEntryRef(out int index)
    {
        ref var result = ref Unsafe.NullRef<Entry>();
        if ( ! _freeListTail.HasValue )
        {
            Assume.False( _freeListHead.HasValue );

            index = Count;
            if ( Count == _items.Length )
            {
                Count = checked( Count + 1 );
                var prevItems = _items;
                _items = new Entry[Buffers.GetCapacity( Count )];
                prevItems.AsSpan().CopyTo( _items );
            }
            else
                ++Count;

            Assume.True( _items[index].IsUnused );
            result = ref GetEntryRef( index );
        }
        else
        {
            Assume.IsLessThan( Count, _items.Length );
            Assume.True( _freeListHead.HasValue );

            ++Count;
            index = _freeListTail.Value;
            result = ref GetEntryRef( index );

            if ( ! result.Prev.HasValue )
                _freeListHead = _freeListTail = NullableIndex.Null;
            else
            {
                ref var prev = ref GetEntryRef( result.Prev.Value );
                Assume.True( prev.IsInFreeList );
                Assume.Equals( prev.Next, _freeListTail );
                prev.MakeFree( prev.Prev, NullableIndex.Null );
                _freeListTail = result.Prev;
            }
        }

        return ref result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UnlinkFirstNode(ref Entry entry)
    {
        var nextIndex = entry.Next;
        if ( nextIndex.HasValue )
        {
            Assume.NotEquals( _occupiedListHead, _occupiedListTail );
            ref var next = ref GetEntryRef( nextIndex.Value );
            next.MakeOccupied( NullableIndex.Null, next.Next );
            _occupiedListHead = nextIndex;
        }
        else
        {
            Assume.Equals( _occupiedListHead, _occupiedListTail );
            _occupiedListHead = _occupiedListTail = NullableIndex.Null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void UnlinkLastNode(ref Entry entry)
    {
        var prevIndex = entry.Prev;
        if ( prevIndex.HasValue )
        {
            Assume.NotEquals( _occupiedListHead, _occupiedListTail );
            ref var prev = ref GetEntryRef( prevIndex.Value );
            prev.MakeOccupied( prev.Prev, NullableIndex.Null );
            _occupiedListTail = prevIndex;
        }
        else
        {
            Assume.Equals( _occupiedListHead, _occupiedListTail );
            _occupiedListHead = _occupiedListTail = NullableIndex.Null;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RemoveEntry(int index, ref Entry entry)
    {
        entry.Value = default!;
        if ( _freeListTail.HasValue )
        {
            Assume.True( _freeListHead.HasValue );
            ref var tail = ref GetEntryRef( _freeListTail.Value );
            tail.MakeFree( tail.Prev, NullableIndex.Create( index ) );
            entry.MakeFree( _freeListTail, NullableIndex.Null );
            _freeListTail = NullableIndex.Create( index );
        }
        else
        {
            Assume.False( _freeListHead.HasValue );
            entry.MakeFree( NullableIndex.Null, NullableIndex.Null );
            _freeListHead = _freeListTail = NullableIndex.Create( index );
        }

        --Count;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int FindMaxOccupiedIndex()
    {
        var result = -1;
        if ( ! _occupiedListHead.HasValue )
            return result;

        var next = _occupiedListHead;
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        do
        {
            if ( result < next )
                result = next.Value;

            ref var entry = ref Unsafe.Add( ref first, next.Value );
            next = entry.Next;
        }
        while ( next.HasValue );

        return result;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RebuildFreeList(int endIndex)
    {
        _freeListHead = _freeListTail = NullableIndex.Null;
        var remaining = endIndex - Count;
        if ( remaining <= 0 )
            return;

        var index = 0;
        ref var first = ref MemoryMarshal.GetArrayDataReference( _items );
        ref var entry = ref first;

        while ( ! entry.IsInFreeList )
        {
            Assume.IsLessThan( index, endIndex );
            entry = ref Unsafe.Add( ref entry, 1 );
            ++index;
        }

        entry.MakeFree( NullableIndex.Null, NullableIndex.Null );
        _freeListHead = _freeListTail = NullableIndex.Create( index );
        --remaining;

        while ( remaining > 0 )
        {
            Assume.IsLessThan( index, endIndex );
            entry = ref Unsafe.Add( ref entry, 1 );
            ++index;

            if ( ! entry.IsInFreeList )
                continue;

            entry.MakeFree( NullableIndex.Null, _freeListHead );
            ref var head = ref Unsafe.Add( ref first, _freeListHead.Value );
            _freeListHead = NullableIndex.Create( index );
            head.MakeFree( _freeListHead, head.Next );
            --remaining;
        }
    }
}
