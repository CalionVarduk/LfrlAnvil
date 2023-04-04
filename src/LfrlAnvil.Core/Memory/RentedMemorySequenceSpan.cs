using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Memory;

public readonly struct RentedMemorySequenceSpan<T> : IReadOnlyList<T>, ICollection<T>
{
    public static readonly RentedMemorySequenceSpan<T> Empty = new RentedMemorySequenceSpan<T>( null, 0, 0 );

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
            return _node.GetElement( StartIndex + index );
        }
        set
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Ensure.IsLessThan( index, Length, nameof( index ) );
            Assume.IsNotNull( _node, nameof( _node ) );
            _node.SetElement( StartIndex + index, value );
        }
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
    public MemorySequenceEnumerator<T> GetEnumerator()
    {
        return new MemorySequenceEnumerator<T>( _node, StartIndex, Length );
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
