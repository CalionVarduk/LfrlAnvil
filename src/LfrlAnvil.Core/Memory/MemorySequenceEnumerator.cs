using System;
using System.Collections;
using System.Collections.Generic;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

public struct MemorySequenceEnumerator<T> : IEnumerator<T>
{
    private readonly int _offset;
    private int _length;
    private MemorySequencePool<T>.Node? _node;
    private ArraySequenceIndex _index;
    private T[] _currentSegment;
    private int _remaining;

    internal MemorySequenceEnumerator(MemorySequencePool<T>.Node? node, int offset, int length)
    {
        _node = node;
        _offset = offset;
        _length = length;
        _remaining = length;
        Initialize( _node, _offset, out _index, out _currentSegment );
    }

    public T Current => _currentSegment[_index.Element];
    object? IEnumerator.Current => Current;

    public bool MoveNext()
    {
        if ( _remaining == 0 )
            return false;

        Assume.IsNotNull( _node, nameof( _node ) );
        Assume.Equals( _node.IsFree, false, nameof( _node.IsFree ) );

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
