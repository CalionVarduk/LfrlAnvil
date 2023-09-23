using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

public readonly ref struct RentedMemorySequenceSpan<T>
{
    public static RentedMemorySequenceSpan<T> Empty => default;

    private readonly MemorySequencePool<T>.Node? _node;

    internal RentedMemorySequenceSpan(MemorySequencePool<T>.Node? node, int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0, nameof( startIndex ) );
        Ensure.IsGreaterThanOrEqualTo( length, 0, nameof( length ) );

        var endIndex = checked( startIndex + length );
        Ensure.IsLessThanOrEqualTo( endIndex, node?.Length ?? 0, nameof( endIndex ) );

        _node = node;
        StartIndex = startIndex;
        Length = length;
    }

    public int StartIndex { get; }
    public int Length { get; }

    public RentedMemorySequenceSegmentCollection<T> Segments =>
        _node is null || _node.IsReusable || Length == 0
            ? RentedMemorySequenceSegmentCollection<T>.Empty
            : new RentedMemorySequenceSegmentCollection<T>( _node, StartIndex, Length );

    public T this[int index]
    {
        get
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Ensure.IsLessThan( index, Length, nameof( index ) );
            Assume.IsNotNull( _node, nameof( _node ) );
            return _node.GetElement( StartIndex + index );
        }
        set => Set( index, value );
    }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( RentedMemorySequenceSpan<T> )}<{typeof( T ).Name}>[{Length}]";
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public RentedMemorySequenceSpan<T> Slice(int startIndex)
    {
        return Slice( startIndex, Length - startIndex );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public RentedMemorySequenceSpan<T> Slice(int startIndex, int length)
    {
        return new RentedMemorySequenceSpan<T>( _node, StartIndex + startIndex, length );
    }

    [Pure]
    public bool Contains(T item)
    {
        return _node is not null && ! _node.IsReusable && _node.IndexOf( item, StartIndex, Length ) != -1;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ref T GetRef(int index)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
        Ensure.IsLessThan( index, Length, nameof( index ) );
        Assume.IsNotNull( _node, nameof( _node ) );
        return ref _node.GetElementRef( StartIndex + index );
    }

    public void Set(int index, T item)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
        Ensure.IsLessThan( index, Length, nameof( index ) );
        Assume.IsNotNull( _node, nameof( _node ) );
        _node.SetElement( StartIndex + index, item );
    }

    public void Clear()
    {
        if ( _node is not null && ! _node.IsReusable )
            _node.ClearSegments( StartIndex, Length );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void CopyTo(T[] array, int arrayIndex)
    {
        CopyTo( array.AsSpan( arrayIndex ) );
    }

    public void CopyTo(Span<T> span)
    {
        if ( _node is null || _node.IsReusable )
            return;

        Ensure.IsGreaterThanOrEqualTo( span.Length, Length, nameof( span ) + '.' + nameof( span.Length ) );
        _node.CopyTo( span, StartIndex, Length );
    }

    public void CopyTo(RentedMemorySequenceSpan<T> span)
    {
        if ( _node is null || _node.IsReusable )
            return;

        Ensure.IsGreaterThanOrEqualTo( span.Length, Length, nameof( span ) + '.' + nameof( span.Length ) );

        var offset = span.StartIndex;
        foreach ( var segment in Segments )
        {
            Assume.IsNotNull( span._node, nameof( span._node ) );
            span._node.CopyFrom( segment, offset );
            offset += segment.Count;
        }
    }

    public void CopyFrom(ReadOnlySpan<T> span)
    {
        if ( _node is null || _node.IsReusable )
            return;

        Ensure.IsGreaterThanOrEqualTo( Length, span.Length, nameof( Length ) );
        _node.CopyFrom( span, StartIndex );
    }

    [Pure]
    public T[] ToArray()
    {
        if ( _node is null || _node.IsReusable || Length == 0 )
            return Array.Empty<T>();

        var result = new T[Length];
        _node.CopyTo( result, StartIndex, Length );
        return result;
    }

    public void Sort(Comparer<T>? comparer = null)
    {
        Sort( (comparer ?? Comparer<T>.Default).Compare );
    }

    public void Sort(Comparison<T> comparer)
    {
        if ( _node is not null && ! _node.IsReusable )
            _node.Sort( StartIndex, Length, comparer );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _node, StartIndex, Length );
    }

    public ref struct Enumerator
    {
        private readonly MemorySequencePool<T>.Node? _node;
        private ArraySequenceIndex _index;
        private T[] _currentSegment;
        private int _remaining;

        internal Enumerator(MemorySequencePool<T>.Node? node, int offset, int length)
        {
            _node = node;
            _remaining = length;

            if ( node is null )
            {
                _index = ArraySequenceIndex.Zero;
                _currentSegment = Array.Empty<T>();
            }
            else
            {
                var startIndex = node.OffsetFirstIndex( offset );
                if ( startIndex.Element > 0 )
                {
                    _index = new ArraySequenceIndex( startIndex.Segment, startIndex.Element - 1 );
                    _currentSegment = node.GetAbsoluteSegment( _index.Segment );
                }
                else
                {
                    _index = new ArraySequenceIndex( startIndex.Segment - 1, node.Pool.SegmentLength - 1 );
                    _currentSegment = Array.Empty<T>();
                }
            }
        }

        public ref readonly T Current => ref _currentSegment[_index.Element];

        public bool MoveNext()
        {
            if ( _remaining == 0 )
                return false;

            Assume.IsNotNull( _node, nameof( _node ) );
            Assume.Equals( _node.IsReusable, false, nameof( _node.IsReusable ) );

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
    }
}
