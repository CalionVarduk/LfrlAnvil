using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

public struct RentedMemorySequence<T> : IReadOnlyList<T>, ICollection<T>, IDisposable
{
    public static readonly RentedMemorySequence<T> Empty = new RentedMemorySequence<T>();

    private MemorySequencePool<T>.Node? _node;

    internal RentedMemorySequence(MemorySequencePool<T>.Node node)
    {
        Assume.Equals( node.IsReusable, false, nameof( node.IsReusable ) );
        _node = node;
        Length = _node.Length;
    }

    public int Length { get; private set; }
    public MemorySequencePool<T>? Owner => _node?.Pool;

    public RentedMemorySequenceSegmentCollection<T> Segments =>
        _node is null || _node.IsReusable || Length == 0
            ? RentedMemorySequenceSegmentCollection<T>.Empty
            : new RentedMemorySequenceSegmentCollection<T>( _node );

    bool ICollection<T>.IsReadOnly => false;
    int ICollection<T>.Count => Length;
    int IReadOnlyCollection<T>.Count => Length;

    public T this[int index]
    {
        get
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Ensure.IsLessThan( index, Length, nameof( index ) );
            Assume.IsNotNull( _node, nameof( _node ) );
            return _node.GetElement( index );
        }
        set => Set( index, value );
    }

    [Pure]
    public override string ToString()
    {
        return $"{nameof( RentedMemorySequence<T> )}<{typeof( T ).Name}>[{Length}]";
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
        return new RentedMemorySequenceSpan<T>( _node, startIndex, length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ref T GetRef(int index)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
        Ensure.IsLessThan( index, Length, nameof( index ) );
        Assume.IsNotNull( _node, nameof( _node ) );
        return ref _node.GetElementRef( index );
    }

    public void Set(int index, T item)
    {
        Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
        Ensure.IsLessThan( index, Length, nameof( index ) );
        Assume.IsNotNull( _node, nameof( _node ) );
        _node.SetElement( index, item );
    }

    public void Push(T item)
    {
        if ( _node is null || _node.IsReusable )
            return;

        _node = _node.Push( item );
        Length = _node.Length;
    }

    public void Expand(int length)
    {
        if ( length <= 0 || _node is null || _node.IsReusable )
            return;

        _node = _node.Expand( length );
        Length = _node.Length;
    }

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

    public void Dispose()
    {
        if ( _node is null )
            return;

        _node.Free();
        _node = null;
        Length = 0;
    }

    [Pure]
    public bool Contains(T item)
    {
        return _node is not null && ! _node.IsReusable && _node.IndexOf( item ) != -1;
    }

    public void Clear()
    {
        if ( _node is not null && ! _node.IsReusable )
            _node.ClearSegments();
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
        _node.CopyTo( span );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void CopyTo(RentedMemorySequenceSpan<T> span)
    {
        ((RentedMemorySequenceSpan<T>)this).CopyTo( span );
    }

    public void CopyFrom(ReadOnlySpan<T> span)
    {
        if ( _node is null || _node.IsReusable || span.Length == 0 )
            return;

        Ensure.IsGreaterThanOrEqualTo( Length, span.Length, nameof( Length ) );
        _node.CopyFrom( span );
    }

    [Pure]
    public T[] ToArray()
    {
        if ( _node is null || _node.IsReusable || Length == 0 )
            return Array.Empty<T>();

        var result = new T[Length];
        _node.CopyTo( result );
        return result;
    }

    public void Sort(Comparer<T>? comparer = null)
    {
        Sort( (comparer ?? Comparer<T>.Default).Compare );
    }

    public void Sort(Comparison<T> comparer)
    {
        ((RentedMemorySequenceSpan<T>)this).Sort( comparer );
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _node, 0, Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator RentedMemorySequenceSpan<T>(RentedMemorySequence<T> s)
    {
        return s._node is null || s._node.IsReusable
            ? RentedMemorySequenceSpan<T>.Empty
            : new RentedMemorySequenceSpan<T>( s._node, 0, s.Length );
    }

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

        public T Current => _currentSegment[_index.Element];
        object? IEnumerator.Current => Current;

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
