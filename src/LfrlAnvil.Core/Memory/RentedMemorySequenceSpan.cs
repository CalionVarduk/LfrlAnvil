using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

/// <summary>
/// A lightweight slice of <see cref="RentedMemorySequence{T}"/>.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly ref struct RentedMemorySequenceSpan<T>
{
    /// <summary>
    /// An empty sequence span.
    /// </summary>
    public static RentedMemorySequenceSpan<T> Empty => default;

    private readonly MemorySequencePool<T>.Node? _node;

    internal RentedMemorySequenceSpan(MemorySequencePool<T>.Node? node, int startIndex, int length)
    {
        Ensure.IsGreaterThanOrEqualTo( startIndex, 0 );
        Ensure.IsGreaterThanOrEqualTo( length, 0 );

        var endIndex = checked( startIndex + length );
        Ensure.IsLessThanOrEqualTo( endIndex, node?.Length ?? 0 );

        _node = node;
        StartIndex = startIndex;
        Length = length;
    }

    /// <summary>
    /// Index of the first element of a sequence included in this span.
    /// </summary>
    public int StartIndex { get; }

    /// <summary>
    /// Number of elements in this span.
    /// </summary>
    public int Length { get; }

    /// <summary>
    /// Creates a new <see cref="RentedMemorySequenceSegmentCollection{T}"/> instance from this span.
    /// </summary>
    public RentedMemorySequenceSegmentCollection<T> Segments =>
        _node is null || _node.IsReusable || Length == 0
            ? RentedMemorySequenceSegmentCollection<T>.Empty
            : new RentedMemorySequenceSegmentCollection<T>( _node, StartIndex, Length );

    /// <summary>
    /// Gets or sets the element at the specified position in this span.
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
            return _node.GetElement( StartIndex + index );
        }
        set => Set( index, value );
    }

    /// <summary>
    /// Returns a string representation of this <see cref="RentedMemorySequenceSpan{T}"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"{nameof( RentedMemorySequenceSpan<T> )}<{typeof( T ).Name}>[{Length}]";
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
        return new RentedMemorySequenceSpan<T>( _node, StartIndex + startIndex, length );
    }

    /// <summary>
    /// Checks whether or not the provided <paramref name="item"/> exists in this span.
    /// </summary>
    /// <param name="item">Item to check.</param>
    /// <returns><b>true</b> when the provided <paramref name="item"/> exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool Contains(T item)
    {
        return _node is not null && ! _node.IsReusable && _node.IndexOf( item, StartIndex, Length ) != -1;
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
        return ref _node.GetElementRef( StartIndex + index );
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
        _node.SetElement( StartIndex + index, item );
    }

    /// <summary>
    /// Sets all elements to the default value in this span.
    /// </summary>
    public void Clear()
    {
        if ( _node is not null && ! _node.IsReusable )
            _node.ClearSegments( StartIndex, Length );
    }

    /// <summary>
    /// Copies this span to the provided <paramref name="array"/>, starting from the given <paramref name="arrayIndex"/>.
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
    /// Copies this span to the provided <paramref name="span"/>.
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
        _node.CopyTo( span, StartIndex, Length );
    }

    /// <summary>
    /// Copies this span to the provided <paramref name="span"/>.
    /// </summary>
    /// <param name="span">Copy destination.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <see cref="Length"/> is greater than length of the provided <paramref name="span"/>.
    /// </exception>
    public void CopyTo(RentedMemorySequenceSpan<T> span)
    {
        if ( _node is null || _node.IsReusable )
            return;

        Ensure.IsGreaterThanOrEqualTo( span.Length, Length );

        var offset = span.StartIndex;
        foreach ( var segment in Segments )
        {
            Assume.IsNotNull( span._node );
            span._node.CopyFrom( segment, offset );
            offset += segment.Count;
        }
    }

    /// <summary>
    /// Copies the provided <paramref name="span"/> to this span.
    /// </summary>
    /// <param name="span">Copy source.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <see cref="Length"/> is less than length of the provided <paramref name="span"/>.
    /// </exception>
    /// <remarks>Does nothing when this span has been returned to the pool.</remarks>
    public void CopyFrom(ReadOnlySpan<T> span)
    {
        if ( _node is null || _node.IsReusable )
            return;

        Ensure.IsGreaterThanOrEqualTo( Length, span.Length );
        _node.CopyFrom( span, StartIndex );
    }

    /// <summary>
    /// Creates a new <see cref="Array"/> instance from this span.
    /// </summary>
    /// <returns>New <see cref="Array"/> instance.</returns>
    [Pure]
    public T[] ToArray()
    {
        if ( _node is null || _node.IsReusable || Length == 0 )
            return Array.Empty<T>();

        var result = new T[Length];
        _node.CopyTo( result, StartIndex, Length );
        return result;
    }

    /// <summary>
    /// Sorts elements in this span.
    /// </summary>
    /// <param name="comparer">Optional comparer to use for sorting elements.</param>
    public void Sort(Comparer<T>? comparer = null)
    {
        Sort( (comparer ?? Comparer<T>.Default).Compare );
    }

    /// <summary>
    /// Sorts elements in this span.
    /// </summary>
    /// <param name="comparer">Delegate used for sorting elements.</param>
    public void Sort(Comparison<T> comparer)
    {
        if ( _node is not null && ! _node.IsReusable )
            _node.Sort( StartIndex, Length, comparer );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this span.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _node, StartIndex, Length );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="RentedMemorySequenceSpan{T}"/>.
    /// </summary>
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

        /// <summary>
        /// Gets reference to the element in the <see cref="RentedMemorySequenceSpan{T}"/> at the current position of this enumerator.
        /// </summary>
        public ref readonly T Current => ref _currentSegment[_index.Element];

        /// <summary>
        /// Advances this enumerator to the next element.
        /// </summary>
        /// <returns><b>true</b> when next element exists, otherwise <b>false</b>.</returns>
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
    }
}
