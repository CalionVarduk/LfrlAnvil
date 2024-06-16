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
using LfrlAnvil.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

/// <summary>
/// A lightweight container for an underlying <see cref="MemorySequencePool{T}"/> node.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public struct RentedMemorySequence<T> : IReadOnlyList<T>, ICollection<T>, IDisposable
{
    /// <summary>
    /// An empty sequence. This instance cannot be used to create larger sequences.
    /// </summary>
    public static readonly RentedMemorySequence<T> Empty = new RentedMemorySequence<T>();

    private MemorySequencePool<T>.Node? _node;

    internal RentedMemorySequence(MemorySequencePool<T>.Node node)
    {
        Assume.False( node.IsReusable );
        _node = node;
        Length = _node.Length;
    }

    /// <summary>
    /// Number of elements in this sequence.
    /// </summary>
    public int Length { get; private set; }

    /// <summary>
    /// <see cref="MemorySequencePool{T}"/> instance that owns this sequence.
    /// </summary>
    public MemorySequencePool<T>? Owner => _node?.Pool;

    /// <summary>
    /// Creates a new <see cref="RentedMemorySequenceSegmentCollection{T}"/> instance from this sequence.
    /// </summary>
    public RentedMemorySequenceSegmentCollection<T> Segments =>
        _node is null || _node.IsReusable || Length == 0
            ? RentedMemorySequenceSegmentCollection<T>.Empty
            : new RentedMemorySequenceSegmentCollection<T>( _node );

    bool ICollection<T>.IsReadOnly => false;
    int ICollection<T>.Count => Length;
    int IReadOnlyCollection<T>.Count => Length;

    /// <summary>
    /// Gets or sets the element at the specified position in this sequence.
    /// </summary>
    /// <param name="index">The zero-based index of the element to get or set.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> is less than <b>0</b> or greater than or equal to <see cref="Length"/>.
    /// </exception>
    public T this[int index]
    {
        get
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0 );
            Ensure.IsLessThan( index, Length );
            Assume.IsNotNull( _node );
            return _node.GetElement( index );
        }
        set => Set( index, value );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="RentedMemorySequence{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( RentedMemorySequence<T> )}<{typeof( T ).Name}>[{Length}]";
    }

    /// <summary>
    /// Creates a new <see cref="RentedMemorySequenceSpan{T}"/> instance.
    /// </summary>
    /// <param name="startIndex">Index of the first element that should be included in the slice.</param>
    /// <returns>New <see cref="RentedMemorySequenceSpan{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="startIndex"/> is less than <b>0</b> or is greater than or equal to <see cref="Length"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public RentedMemorySequenceSpan<T> Slice(int startIndex)
    {
        return Slice( startIndex, Length - startIndex );
    }

    /// <summary>
    /// Creates a new <see cref="RentedMemorySequenceSpan{T}"/> instance.
    /// </summary>
    /// <param name="startIndex">Index of the first element that should be included in the slice.</param>
    /// <param name="length">Length of the slice.</param>
    /// <returns>New <see cref="RentedMemorySequenceSpan{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="startIndex"/> is less than <b>0</b>
    /// or <paramref name="length"/> is less than <b>0</b>
    /// or computed index of the last element in the slice is greater than or equal to <see cref="Length"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public RentedMemorySequenceSpan<T> Slice(int startIndex, int length)
    {
        return new RentedMemorySequenceSpan<T>( _node, startIndex, length );
    }

    /// <summary>
    /// Returns a reference to an element at the specified position.
    /// </summary>
    /// <param name="index">The zero-based index of the element reference to get.</param>
    /// <returns>Reference to an element at the specified position.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> is less than <b>0</b> or greater than or equal to <see cref="Length"/>.
    /// </exception>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ref T GetRef(int index)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0 );
        Ensure.IsLessThan( index, Length );
        Assume.IsNotNull( _node );
        return ref _node.GetElementRef( index );
    }

    /// <summary>
    /// Sets an element at the specified position.
    /// </summary>
    /// <param name="index">The zero-based index of the element to set.</param>
    /// <param name="item">Element to set.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> is less than <b>0</b> or greater than or equal to <see cref="Length"/>.
    /// </exception>
    public void Set(int index, T item)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0 );
        Ensure.IsLessThan( index, Length );
        Assume.IsNotNull( _node );
        _node.SetElement( index, item );
    }

    /// <summary>
    /// Increases <see cref="Length"/> of this sequence by <b>1</b> and sets the last element.
    /// </summary>
    /// <param name="item">Element to add.</param>
    /// <remarks>Does nothing when this sequence has been returned to the pool.</remarks>
    public void Push(T item)
    {
        if ( _node is null || _node.IsReusable )
            return;

        _node = _node.Push( item );
        Length = _node.Length;
    }

    /// <summary>
    /// Increases <see cref="Length"/> of this sequence by the provided <paramref name="length"/>.
    /// </summary>
    /// <param name="length">Value to increase <see cref="Length"/> by.</param>
    /// <remarks>Does nothing when this sequence has been returned to the pool or <paramref name="length"/> is less than <b>1</b>.</remarks>
    public void Expand(int length)
    {
        if ( length <= 0 || _node is null || _node.IsReusable )
            return;

        _node = _node.Expand( length );
        Length = _node.Length;
    }

    /// <summary>
    /// Refreshes this sequence's <see cref="Length"/>.
    /// </summary>
    /// <remarks>
    /// Some operations may cause the underlying node to be modified.
    /// This method can be used to synchronize a sequence with it's node's state.
    /// </remarks>
    public void Refresh()
    {
        if ( _node is null )
            return;

        if ( ! _node.IsReusable )
        {
            Length = _node.Length;
            return;
        }

        _node = null;
        Length = 0;
    }

    /// <inheritdoc />
    /// <remarks>Frees the underlying node and returns it to the pool.</remarks>
    public void Dispose()
    {
        if ( _node is null )
            return;

        _node.Free();
        _node = null;
        Length = 0;
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="item"/> exists in this sequence.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="item"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(T item)
    {
        return _node is not null && ! _node.IsReusable && _node.IndexOf( item ) != -1;
    }

    /// <summary>
    /// Sets all elements to the default value in this sequence.
    /// </summary>
    public void Clear()
    {
        if ( _node is not null && ! _node.IsReusable )
            _node.ClearSegments();
    }

    /// <summary>
    /// Copies this sequence to the provided <paramref name="array"/>, starting from the given <paramref name="arrayIndex"/>.
    /// </summary>
    /// <param name="array">Copy destination.</param>
    /// <param name="arrayIndex">Index of the destination <paramref name="array"/> at which to start copying to.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="arrayIndex"/> is less than <b>0</b> or greater than or equal to
    /// the length of the provided <paramref name="array"/>
    /// or <see cref="Length"/> is greater than length of the provided <paramref name="array"/> segment.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo( array.AsSpan( arrayIndex ) );
    }

    /// <summary>
    /// Copies this sequence to the provided <paramref name="span"/>.
    /// </summary>
    /// <param name="span">Copy destination.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <see cref="Length"/> is greater than length of the provided <paramref name="span"/>.
    /// </exception>
    public void CopyTo(Span<T> span)
    {
        if ( _node is null || _node.IsReusable )
            return;

        Ensure.IsGreaterThanOrEqualTo( span.Length, Length );
        _node.CopyTo( span );
    }

    /// <summary>
    /// Copies this sequence to the provided <paramref name="span"/>.
    /// </summary>
    /// <param name="span">Copy destination.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <see cref="Length"/> is greater than length of the provided <paramref name="span"/>.
    /// </exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void CopyTo(RentedMemorySequenceSpan<T> span)
    {
        (( RentedMemorySequenceSpan<T> )this).CopyTo( span );
    }

    /// <summary>
    /// Copies the provided <paramref name="span"/> to this sequence.
    /// </summary>
    /// <param name="span">Copy source.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <see cref="Length"/> is less than length of the provided <paramref name="span"/>.
    /// </exception>
    /// <remarks>Does nothing when this sequence has been returned to the pool.</remarks>
    public void CopyFrom(ReadOnlySpan<T> span)
    {
        if ( _node is null || _node.IsReusable || span.Length == 0 )
            return;

        Ensure.IsGreaterThanOrEqualTo( Length, span.Length );
        _node.CopyFrom( span );
    }

    /// <summary>
    /// Creates a new <see cref="Array"/> instance from this sequence.
    /// </summary>
    /// <returns>New <see cref="Array"/> instance.</returns>
    [Pure]
    public T[] ToArray()
    {
        if ( _node is null || _node.IsReusable || Length == 0 )
            return Array.Empty<T>();

        var result = new T[Length];
        _node.CopyTo( result );
        return result;
    }

    /// <summary>
    /// Sorts elements in this sequence.
    /// </summary>
    /// <param name="comparer">Optional comparer to use for sorting elements.</param>
    public void Sort(Comparer<T>? comparer = null)
    {
        Sort( (comparer ?? Comparer<T>.Default).Compare );
    }

    /// <summary>
    /// Sorts elements in this sequence.
    /// </summary>
    /// <param name="comparer">Delegate used for sorting elements.</param>
    public void Sort(Comparison<T> comparer)
    {
        (( RentedMemorySequenceSpan<T> )this).Sort( comparer );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this sequence.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _node, 0, Length );
    }

    /// <summary>
    /// Converts a sequence to an equivalent <see cref="RentedMemorySequenceSpan{T}"/> instance.
    /// </summary>
    /// <param name="s">Source sequence.</param>
    /// <returns>New <see cref="RentedMemorySequenceSpan{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator RentedMemorySequenceSpan<T>(RentedMemorySequence<T> s)
    {
        return s._node is null || s._node.IsReusable
            ? RentedMemorySequenceSpan<T>.Empty
            : new RentedMemorySequenceSpan<T>( s._node, 0, s.Length );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="RentedMemorySequence{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly int _offset;
        private int _length;
        private MemorySequencePool<T>.Node? _node;
        private ArraySequenceIndex _index;
        private T[] _currentSegment;
        private int _remaining;

        internal Enumerator(MemorySequencePool<T>.Node? node, int offset, int length)
        {
            _node = node;
            _offset = offset;
            _length = length;
            _remaining = length;
            Initialize( _node, _offset, out _index, out _currentSegment );
        }

        /// <inheritdoc />
        public T Current => _currentSegment[_index.Element];

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        public bool MoveNext()
        {
            if ( _remaining == 0 )
                return false;

            Assume.IsNotNull( _node );
            Assume.False( _node.IsReusable );

            --_remaining;
            var elementIndex = _index.Element + 1;
            if ( elementIndex < _node.Pool.SegmentLength )
                _index = new ArraySequenceIndex( _index.Segment, elementIndex );
            else
            {
                _index = new ArraySequenceIndex( _index.Segment + 1, 0 );
                _currentSegment = _node.GetAbsoluteSegment( _index.Segment );
            }

            return true;
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _node = null;
            _length = 0;
            _index = ArraySequenceIndex.Zero;
            _currentSegment = Array.Empty<T>();
            _remaining = 0;
        }

        void IEnumerator.Reset()
        {
            _remaining = _length;
            Initialize( _node, _offset, out _index, out _currentSegment );
        }

        private static void Initialize(MemorySequencePool<T>.Node? node, int offset, out ArraySequenceIndex index, out T[] currentSegment)
        {
            if ( node is null )
            {
                index = ArraySequenceIndex.Zero;
                currentSegment = Array.Empty<T>();
                return;
            }

            var startIndex = node.OffsetFirstIndex( offset );
            if ( startIndex.Element > 0 )
            {
                index = new ArraySequenceIndex( startIndex.Segment, startIndex.Element - 1 );
                currentSegment = node.GetAbsoluteSegment( index.Segment );
            }
            else
            {
                index = new ArraySequenceIndex( startIndex.Segment - 1, node.Pool.SegmentLength - 1 );
                currentSegment = Array.Empty<T>();
            }
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

    void ICollection<T>.Add(T item)
    {
        throw new NotSupportedException( ExceptionResources.FixedSizeCollection );
    }

    bool ICollection<T>.Remove(T item)
    {
        throw new NotSupportedException( ExceptionResources.FixedSizeCollection );
    }
}
