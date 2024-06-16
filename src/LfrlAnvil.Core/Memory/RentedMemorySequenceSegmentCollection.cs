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
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

/// <summary>
/// A lightweight collection of <see cref="RentedMemorySequence{T}"/> or <see cref="RentedMemorySequenceSpan{T}"/> segment slices.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly ref struct RentedMemorySequenceSegmentCollection<T>
{
    /// <summary>
    /// An empty sequence segment collection.
    /// </summary>
    public static RentedMemorySequenceSegmentCollection<T> Empty => default;

    private readonly MemorySequencePool<T>.Node? _node;
    private readonly ArraySequenceIndex _first;
    private readonly ArraySequenceIndex _last;

    internal RentedMemorySequenceSegmentCollection(MemorySequencePool<T>.Node node)
    {
        _node = node;
        _first = _node.FirstIndex;
        _last = _node.LastIndex;
        Length = _last.Segment - _first.Segment + 1;
    }

    internal RentedMemorySequenceSegmentCollection(MemorySequencePool<T>.Node node, int startIndex, int length)
    {
        _node = node;
        _first = _node.OffsetFirstIndex( startIndex );
        _last = _node.OffsetFirstIndex( startIndex + length - 1 );
        Length = _last.Segment - _first.Segment + 1;
    }

    /// <summary>
    /// Total number of fully or partially occupied segments.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Gets the full or partial segment at the specified position in this sequence segment collection.
    /// </summary>
    /// <param name="index">The zero-based index of the segment to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="index"/> is less than <b>0</b> or greater than or equal to <see cref="Length"/>.
    /// </exception>
    public ArraySegment<T> this[int index]
    {
        get
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0 );
            Ensure.IsLessThan( index, Length );
            Assume.IsNotNull( _node );

            if ( index == 0 )
            {
                var segment = _node.GetAbsoluteSegment( _first.Segment );

                return _first.Segment == _last.Segment
                    ? new ArraySegment<T>( segment, _first.Element, Length )
                    : new ArraySegment<T>( segment, _first.Element, segment.Length - _first.Element );
            }

            index += _first.Segment;
            return index == _last.Segment
                ? new ArraySegment<T>( _node.GetAbsoluteSegment( index ), 0, _last.Element + 1 )
                : _node.GetAbsoluteSegment( index );
        }
    }

    /// <summary>
    /// Returns a string representation of this <see cref="RentedMemorySequenceSegmentCollection{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( RentedMemorySequenceSegmentCollection<T> )}<{typeof( T ).Name}>[{Length}]";
    }

    /// <summary>
    /// Creates a new <see cref="Array"/> instance from this sequence segment collection.
    /// </summary>
    /// <returns>New <see cref="Array"/> instance.</returns>
    [Pure]
    public ArraySegment<T>[] ToArray()
    {
        if ( Length == 0 )
            return Array.Empty<ArraySegment<T>>();

        var index = 0;
        var result = new ArraySegment<T>[Length];
        foreach ( var segment in this )
            result[index++] = segment;

        return result;
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this sequence segment collection.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _node, _first, _last, Length );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="RentedMemorySequenceSegmentCollection{T}"/>.
    /// </summary>
    public ref struct Enumerator
    {
        private const byte FirstSegmentState = 0;
        private const byte NextSegmentState = 1;
        private const byte DoneState = 2;

        private readonly MemorySequencePool<T>.Node? _node;
        private readonly ArraySequenceIndex _first;
        private readonly ArraySequenceIndex _last;
        private int _nextIndex;
        private byte _state;

        internal Enumerator(MemorySequencePool<T>.Node? node, ArraySequenceIndex first, ArraySequenceIndex last, int length)
        {
            _node = node;
            _first = first;
            _last = last;
            _nextIndex = _first.Segment;
            _state = _node is null || length == 0 ? DoneState : FirstSegmentState;
            Current = ArraySegment<T>.Empty;
        }

        /// <summary>
        /// Gets the segment in the <see cref="RentedMemorySequenceSegmentCollection{T}"/> at the current position of this enumerator.
        /// </summary>
        public ArraySegment<T> Current { get; private set; }

        /// <summary>
        /// Advances this enumerator to the next segment.
        /// </summary>
        /// <returns><b>true</b> when next element exists, otherwise <b>false</b>.</returns>
        public bool MoveNext()
        {
            switch ( _state )
            {
                case FirstSegmentState:
                {
                    Assume.IsNotNull( _node );
                    var segment = _node.GetAbsoluteSegment( _nextIndex );

                    if ( _nextIndex == _last.Segment )
                    {
                        Current = new ArraySegment<T>( segment, _first.Element, _last.Element - _first.Element + 1 );
                        _state = DoneState;
                    }
                    else
                    {
                        Current = new ArraySegment<T>( segment, _first.Element, segment.Length - _first.Element );
                        _state = NextSegmentState;
                    }

                    break;
                }
                case NextSegmentState:
                {
                    Assume.IsNotNull( _node );
                    var segment = _node.GetAbsoluteSegment( _nextIndex );

                    if ( _nextIndex < _last.Segment )
                        Current = segment;
                    else
                    {
                        Current = new ArraySegment<T>( segment, 0, _last.Element + 1 );
                        _state = DoneState;
                    }

                    break;
                }
                default:
                    return false;
            }

            ++_nextIndex;
            return true;
        }
    }
}
