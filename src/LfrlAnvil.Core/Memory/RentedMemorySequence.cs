using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Exceptions;

namespace LfrlAnvil.Memory;

public readonly struct RentedMemorySequence<T> : IReadOnlyList<T>, ICollection<T>, IDisposable
{
    public static readonly RentedMemorySequence<T> Empty = new RentedMemorySequence<T>();

    private readonly MemorySequencePool<T>.Node? _node;

    internal RentedMemorySequence(MemorySequencePool<T>.Node node)
    {
        Assume.Equals( node.IsReusable, false, nameof( node.IsReusable ) );
        _node = node;
        Length = _node.Length;
    }

    public int Length { get; }
    public MemorySequencePool<T>? Owner => _node?.Pool;

    public RentedMemorySequenceSegmentCollection<T> Segments =>
        _node is null || _node.IsReusable
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
        set
        {
            Ensure.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Ensure.IsLessThan( index, Length, nameof( index ) );
            Assume.IsNotNull( _node, nameof( _node ) );
            _node.SetElement( index, value );
        }
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

    public void Dispose()
    {
        _node?.Free();
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
        if ( _node is null || _node.IsReusable )
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
    public MemorySequenceEnumerator<T> GetEnumerator()
    {
        return new MemorySequenceEnumerator<T>( _node, 0, Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator RentedMemorySequenceSpan<T>(RentedMemorySequence<T> s)
    {
        return s._node is null || s._node.IsReusable
            ? RentedMemorySequenceSpan<T>.Empty
            : new RentedMemorySequenceSpan<T>( s._node, 0, s.Length );
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
