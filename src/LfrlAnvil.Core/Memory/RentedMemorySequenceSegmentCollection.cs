using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

public readonly struct RentedMemorySequenceSegmentCollection<T> : IReadOnlyList<ArraySegment<T>>
{
    public static readonly RentedMemorySequenceSegmentCollection<T> Empty = default;

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

    public int Length { get; }
    int IReadOnlyCollection<ArraySegment<T>>.Count => Length;

    public ArraySegment<T> this[int index]
    {
        get
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Ensure.IsLessThan( index, Length, nameof( index ) );
            Assume.IsNotNull( _node, nameof( _node ) );

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

    [Pure]
    public override string ToString()
    {
        return $"{nameof( RentedMemorySequenceSegmentCollection<T> )}<{typeof( T ).Name}>[{Length}]";
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _node, _first, _last, Length );
    }

    [Pure]
    IEnumerator<ArraySegment<T>> IEnumerable<ArraySegment<T>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<ArraySegment<T>>
    {
        private const byte FirstSegmentState = 0;
        private const byte NextSegmentState = 1;
        private const byte DoneState = 2;

        private MemorySequencePool<T>.Node? _node;
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

        public ArraySegment<T> Current { get; private set; }
        object IEnumerator.Current => Current;

        public bool MoveNext()
        {
            switch ( _state )
            {
                case FirstSegmentState:
                {
                    Assume.IsNotNull( _node, nameof( _node ) );
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
                    Assume.IsNotNull( _node, nameof( _node ) );
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

        public void Dispose()
        {
            _node = null;
            _state = DoneState;
            Current = ArraySegment<T>.Empty;
        }

        void IEnumerator.Reset()
        {
            _nextIndex = _first.Segment;
            _state = _node is null || _last.Segment - _first.Segment == -1 ? DoneState : FirstSegmentState;
            Current = ArraySegment<T>.Empty;
        }
    }
}
