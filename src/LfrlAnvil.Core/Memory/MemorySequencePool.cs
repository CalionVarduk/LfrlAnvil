﻿// Copyright 2024 Łukasz Furlepa
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
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

/// <summary>
/// Represents a pool of <see cref="RentedMemorySequence{T}"/> instances.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public class MemorySequencePool<T>
{
    /// <summary>
    /// A lightweight <see cref="MemorySequencePool{T}"/> state report container.
    /// </summary>
    public readonly struct ReportInfo
    {
        private readonly MemorySequencePool<T>? _pool;

        internal ReportInfo(MemorySequencePool<T> pool)
        {
            _pool = pool;
        }

        /// <summary>
        /// Total number of allocated pool segments.
        /// </summary>
        public int AllocatedSegments => _pool?._segments.Count ?? 0;

        /// <summary>
        /// Number of active pool segments.
        /// </summary>
        public int ActiveSegments => (_pool?._tailNode?.LastIndex.Segment ?? -1) + 1;

        /// <summary>
        /// Number of cached underlying pool nodes.
        /// </summary>
        public int CachedNodes => _pool?._nodeCache.Count ?? 0;

        /// <summary>
        /// Number of active underlying pool nodes.
        /// </summary>
        public int ActiveNodes
        {
            get
            {
                var count = 0;
                var node = _pool?._tailNode;
                while ( node is not null )
                {
                    ++count;
                    node = node.Prev;
                }

                return count;
            }
        }

        /// <summary>
        /// Number of fragmented underlying pool nodes.
        /// </summary>
        public int FragmentedNodes => _pool?._fragmentationHeap.Count ?? 0;

        /// <summary>
        /// Number of active elements.
        /// </summary>
        public int ActiveElements
        {
            get
            {
                var node = _pool?._tailNode;
                return node is not null ? (node.LastIndex.Segment << _pool!._segmentLengthLog2) + node.LastIndex.Element + 1 : 0;
            }
        }

        /// <summary>
        /// Number of fragmented elements.
        /// </summary>
        public int FragmentedElements
        {
            get
            {
                var count = 0;
                var fragmentationHeapLength = _pool?._fragmentationHeap.Count ?? 0;
                if ( fragmentationHeapLength == 0 )
                    return count;

                for ( var i = 0; i < fragmentationHeapLength; ++i )
                    count += _pool!._fragmentationHeap[i].Length;

                return count;
            }
        }

        /// <summary>
        /// Creates a new <see cref="IEnumerable{T}"/> instance that contains sizes of all fragmented underlying pool nodes.
        /// </summary>
        /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
        [Pure]
        public IEnumerable<int> GetFragmentedNodeSizes()
        {
            var fragmentationHeapLength = _pool?._fragmentationHeap.Count ?? 0;
            for ( var i = 0; i < fragmentationHeapLength; ++i )
                yield return _pool!._fragmentationHeap[i].Length;
        }

        /// <summary>
        /// Creates a new <see cref="IEnumerable{T}"/> instance that contains all currently rented memory sequences.
        /// </summary>
        /// <returns>New <see cref="IEnumerable{T}"/> instance.</returns>
        [Pure]
        public IEnumerable<RentedMemorySequence<T>> GetRentedNodes()
        {
            var node = _pool?._tailNode;
            while ( node is not null )
            {
                if ( node.IsReusable )
                {
                    node = node.Prev;
                    if ( node is null )
                        break;

                    Assume.False( node.IsReusable );
                }

                yield return new RentedMemorySequence<T>( node );

                node = node.Prev;
            }
        }
    }

    private readonly int _segmentLengthLog2;
    private Node? _tailNode;
    private StackSlim<Node> _nodeCache;
    private ListSlim<Node> _fragmentationHeap;
    private ListSlim<T[]> _segments;

    /// <summary>
    /// Creates a new <see cref="MemorySequencePool{T}"/> instance.
    /// </summary>
    /// <param name="minSegmentLength">Minimum single pool segment length. The actual value will be rounded up to a power of two.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="minSegmentLength"/> is less than <b>1</b> or greater than <b>2^30</b>.
    /// </exception>
    public MemorySequencePool(int minSegmentLength)
    {
        Ensure.IsGreaterThan( minSegmentLength, 0 );
        Ensure.IsLessThanOrEqualTo( minSegmentLength, 1 << 30 );

        _segmentLengthLog2 = BitOperations.Log2( BitOperations.RoundUpToPowerOf2( unchecked( ( uint )minSegmentLength ) ) );
        SegmentLength = 1 << _segmentLengthLog2;
        ClearReturnedSequences = true;

        _tailNode = null;
        _nodeCache = StackSlim<Node>.Create();
        _fragmentationHeap = ListSlim<Node>.Create();
        _segments = ListSlim<T[]>.Create();
    }

    /// <summary>
    /// Specifies whether or not this pool clears the contents of memory sequences that get returned to the pool.
    /// </summary>
    public bool ClearReturnedSequences { get; set; }

    /// <summary>
    /// Length of a single pool segment.
    /// </summary>
    public int SegmentLength { get; }

    /// <summary>
    /// Creates a new <see cref="ReportInfo"/> instance.
    /// </summary>
    public ReportInfo Report => new ReportInfo( this );

    /// <summary>
    /// Creates a new <see cref="RentedMemorySequence{T}"/> instance from this pool.
    /// </summary>
    /// <param name="length">Size of the rented sequence.</param>
    /// <returns>
    /// New <see cref="RentedMemorySequence{T}"/> instance,
    /// or <see cref="RentedMemorySequence{T}.Empty"/> when <paramref name="length"/> is less than <b>1</b>.
    /// </returns>
    /// <remarks>This method attempts to reuse fragmented segments.</remarks>
    public RentedMemorySequence<T> Rent(int length)
    {
        return length <= 0 ? RentedMemorySequence<T>.Empty : new RentedMemorySequence<T>( RentNode( length ) );
    }

    /// <summary>
    /// Creates a new <see cref="RentedMemorySequence{T}"/> instance from this pool.
    /// </summary>
    /// <param name="length">Initial size of the rented sequence. Equal to <b>0</b> by default.</param>
    /// <returns>New <see cref="RentedMemorySequence{T}"/> instance.</returns>
    /// <remarks>This method always uses or allocates segments at the tail of the pool.</remarks>
    public RentedMemorySequence<T> GreedyRent(int length = 0)
    {
        length = Math.Max( length, 0 );
        return new RentedMemorySequence<T>( AllocateAtTail( length ) );
    }

    /// <summary>
    /// Attempts to deallocate unused segments at the tail of this pool.
    /// </summary>
    public void TrimExcess()
    {
        var cachedTailSegmentCount = _segments.Count;
        if ( _tailNode is not null )
            cachedTailSegmentCount -= _tailNode.LastIndex.Segment + 1;

        _segments.RemoveLastRange( cachedTailSegmentCount );
        _segments.ResetCapacity();
        _fragmentationHeap.ResetCapacity();
        _nodeCache.Clear();
        _nodeCache.ResetCapacity();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Node RentNode(int length)
    {
        Assume.IsGreaterThan( length, 0 );

        if ( _fragmentationHeap.Count > 0 )
        {
            var largestNode = _fragmentationHeap.First();
            if ( largestNode.Length >= length )
                return AllocateAtLargestFragmentedNode( largestNode, length );
        }

        return AllocateAtTail( length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ArraySequenceIndex OffsetIndex(ArraySequenceIndex index, int offset)
    {
        return index.Add( offset, _segmentLengthLog2 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ArraySequenceIndex GetMinusOneIndex()
    {
        return ArraySequenceIndex.MinusOne( SegmentLength );
    }

    private Node AllocateAtTail(int length)
    {
        var node = Node.CreateTailNode( this, length );
        AllocateSegments( node.LastIndex.Segment );
        return node;
    }

    private Node AllocateAtLargestFragmentedNode(Node node, int length)
    {
        Assume.Equals( node, _fragmentationHeap.First() );
        Assume.IsLessThanOrEqualTo( length, node.Length );

        if ( node.Length == length )
        {
            node.DeactivateFragmentationIndex();
            PopFromFragmentationHeap();
            return node;
        }

        var extractedNode = node.ExtractNode( length );
        return extractedNode;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AllocateSegments(int lastSegmentIndex)
    {
        Assume.IsNotNull( _tailNode );

        var segmentsToAllocate = lastSegmentIndex - _segments.Count + 1;
        for ( var i = 0; i < segmentsToAllocate; ++i )
            _segments.Add( new T[SegmentLength] );
    }

    private void AddToFragmentationHeap(Node node)
    {
        Assume.IsGreaterThan( node.Length, 0 );
        Assume.Equals( node.FragmentationIndex, Node.InactiveFragmentationIndex );

        node.UpdateFragmentationIndex( _fragmentationHeap.Count );
        _fragmentationHeap.Add( node );
        FixUpFragmentationHeap( node );
    }

    private void PopFromFragmentationHeap()
    {
        if ( _fragmentationHeap.Count == 1 )
        {
            _fragmentationHeap.RemoveLast();
            return;
        }

        ref var first = ref _fragmentationHeap.First();
        var last = Unsafe.Add( ref first, _fragmentationHeap.Count - 1 );
        first = last;
        last.UpdateFragmentationIndex( 0 );
        _fragmentationHeap.RemoveLast();
        FixDownFragmentationHeap( last );
    }

    private void RemoveFromFragmentationHeap(Node node)
    {
        ref var first = ref _fragmentationHeap.First();
        var last = Unsafe.Add( ref first, _fragmentationHeap.Count - 1 );

        if ( ReferenceEquals( node, last ) )
        {
            node.DeactivateFragmentationIndex();
            _fragmentationHeap.RemoveLast();
            return;
        }

        ref var target = ref Unsafe.Add( ref first, node.FragmentationIndex );
        target = last;

        last.UpdateFragmentationIndex( node.FragmentationIndex );
        node.DeactivateFragmentationIndex();
        _fragmentationHeap.RemoveLast();
        FixRelativeFragmentationHeap( last, node.Length );
    }

    private void FixUpFragmentationHeap(Node node)
    {
        Assume.IsGreaterThanOrEqualTo( node.FragmentationIndex, 0 );
        var p = (node.FragmentationIndex - 1) >> 1;
        ref var first = ref _fragmentationHeap.First();

        while ( node.FragmentationIndex > 0 )
        {
            var parent = Unsafe.Add( ref first, p );
            if ( node.Length <= parent.Length )
                break;

            ref var target = ref Unsafe.Add( ref first, node.FragmentationIndex )!;
            target = parent;

            target = ref Unsafe.Add( ref first, parent.FragmentationIndex )!;
            target = node;

            node.SwapFragmentationIndex( parent );
            p = (p - 1) >> 1;
        }
    }

    private void FixDownFragmentationHeap(Node node)
    {
        Assume.IsGreaterThanOrEqualTo( node.FragmentationIndex, 0 );
        var l = (node.FragmentationIndex << 1) + 1;
        ref var first = ref _fragmentationHeap.First();

        while ( l < _fragmentationHeap.Count )
        {
            var child = Unsafe.Add( ref first, l );
            var nodeToSwap = node.Length < child.Length ? child : node;

            var r = l + 1;
            if ( r < _fragmentationHeap.Count )
            {
                child = Unsafe.Add( ref first, r );
                if ( nodeToSwap.Length < child.Length )
                    nodeToSwap = child;
            }

            if ( ReferenceEquals( node, nodeToSwap ) )
                break;

            ref var target = ref Unsafe.Add( ref first, node.FragmentationIndex )!;
            target = nodeToSwap;

            target = ref Unsafe.Add( ref first, nodeToSwap.FragmentationIndex )!;
            target = node;

            node.SwapFragmentationIndex( nodeToSwap );
            l = (node.FragmentationIndex << 1) + 1;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixRelativeFragmentationHeap(Node node, int oldLength)
    {
        if ( node.Length < oldLength )
            FixDownFragmentationHeap( node );
        else
            FixUpFragmentationHeap( node );
    }

    internal sealed class Node
    {
        internal const int CachedFragmentationIndex = -2;
        internal const int InactiveFragmentationIndex = -1;

        private Node(MemorySequencePool<T> pool)
        {
            Pool = pool;
            FirstIndex = ArraySequenceIndex.Zero;
            LastIndex = pool.GetMinusOneIndex();
            Length = 0;
            FragmentationIndex = InactiveFragmentationIndex;
            Prev = null;
            Next = null;
        }

        internal MemorySequencePool<T> Pool { get; }
        internal ArraySequenceIndex FirstIndex { get; private set; }
        internal ArraySequenceIndex LastIndex { get; private set; }
        internal int Length { get; private set; }
        internal int FragmentationIndex { get; private set; }
        internal Node? Prev { get; private set; }
        internal Node? Next { get; private set; }
        internal bool IsReusable => FragmentationIndex != InactiveFragmentationIndex;

        [Pure]
        public override string ToString()
        {
            return $"({FirstIndex}) : ({LastIndex}) [{Length}], {nameof( FragmentationIndex )} = {FragmentationIndex}";
        }

        internal static Node CreateTailNode(MemorySequencePool<T> pool, int length)
        {
            var firstIndex = pool._tailNode is null ? ArraySequenceIndex.Zero : pool.OffsetIndex( pool._tailNode.LastIndex, 1 );
            var node = GetOrCreateNode( pool );
            node.FirstIndex = firstIndex;
            node.Length = length;
            node.LastIndex = length == 0 ? node.FirstIndex.Decrement( pool.SegmentLength ) : node.OffsetFirstIndex( length - 1 );

            if ( pool._tailNode is not null )
            {
                Assume.IsNull( pool._tailNode.Next );
                node.Prev = pool._tailNode;
                node.Prev.Next = node;
            }

            pool._tailNode = node;
            return node;
        }

        internal Node ExtractNode(int length)
        {
            Assume.IsGreaterThan( length, 0 );
            Assume.IsGreaterThanOrEqualTo( FragmentationIndex, 0 );

            var node = GetOrCreateNode( Pool );
            node.FirstIndex = FirstIndex;
            node.Length = length;
            node.LastIndex = OffsetFirstIndex( length - 1 );

            node.Prev = Prev;
            if ( Prev is not null )
                Prev.Next = node;

            Prev = node;
            node.Next = this;

            DecreaseFragmentedLength( length );
            return node;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ArraySequenceIndex OffsetFirstIndex(int offset)
        {
            return Pool.OffsetIndex( FirstIndex, offset );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T[] GetAbsoluteSegment(int index)
        {
            ref var first = ref Pool._segments.First();
            return Unsafe.Add( ref first, index );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T GetElement(int index)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0 );
            Assume.IsLessThan( index, Length );

            var sequenceIndex = OffsetFirstIndex( index );
            var segment = GetAbsoluteSegment( sequenceIndex.Segment );
            return segment[sequenceIndex.Element];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetElement(int index, T value)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0 );
            Assume.IsLessThan( index, Length );

            var sequenceIndex = OffsetFirstIndex( index );
            var segment = GetAbsoluteSegment( sequenceIndex.Segment );
            segment[sequenceIndex.Element] = value;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ref T GetElementRef(int index)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0 );
            Assume.IsLessThan( index, Length );

            var sequenceIndex = OffsetFirstIndex( index );
            var segment = GetAbsoluteSegment( sequenceIndex.Segment );
            return ref segment[sequenceIndex.Element];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearSegments()
        {
            if ( Length > 0 )
                ClearSegments( FirstIndex, LastIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearSegments(int startIndex, int length)
        {
            if ( length == 0 )
                return;

            var first = OffsetFirstIndex( startIndex );
            var last = Pool.OffsetIndex( first, length - 1 );
            ClearSegments( first, last );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int IndexOf(T item)
        {
            return Length == 0 ? -1 : IndexOf( item, FirstIndex, LastIndex );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int IndexOf(T item, int startIndex, int length)
        {
            if ( length == 0 )
                return -1;

            var first = OffsetFirstIndex( startIndex );
            var last = Pool.OffsetIndex( first, length - 1 );
            return IndexOf( item, first, last );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CopyTo(Span<T> span)
        {
            if ( Length > 0 )
                CopyTo( span, FirstIndex, LastIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CopyTo(Span<T> span, int startIndex, int length)
        {
            if ( length == 0 )
                return;

            var first = OffsetFirstIndex( startIndex );
            var last = Pool.OffsetIndex( first, length - 1 );
            CopyTo( span, first, last );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CopyFrom(ReadOnlySpan<T> span)
        {
            CopyFrom( span, FirstIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CopyFrom(ReadOnlySpan<T> span, int startIndex)
        {
            var first = OffsetFirstIndex( startIndex );
            CopyFrom( span, first );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Node Push(T item)
        {
            var node = Expand( 1 );
            var segment = node.GetAbsoluteSegment( node.LastIndex.Segment );
            segment[node.LastIndex.Element] = item;
            return node;
        }

        internal Node Expand(int length)
        {
            Assume.IsGreaterThan( length, 0 );

            if ( Next is null )
            {
                Length += length;
                LastIndex = Pool.OffsetIndex( LastIndex, length );
                Pool.AllocateSegments( LastIndex.Segment );
                return this;
            }

            if ( ! Next.IsReusable || Next.Length < length )
            {
                var node = Pool.RentNode( Length + length );
                new RentedMemorySequence<T>( this ).CopyTo( new RentedMemorySequence<T>( node ) );
                Free();
                return node;
            }

            if ( Next.Length > length )
                Next.DecreaseFragmentedLength( length );
            else
            {
                Pool.RemoveFromFragmentationHeap( Next );

                var next = Next.Next;
                Assume.IsNotNull( next );

                Next.Next = null;
                Next.Prev = null;
                Next.Deactivate();

                Next = next;
                next.Prev = this;
            }

            Length += length;
            LastIndex = Pool.OffsetIndex( LastIndex, length );
            return this;
        }

        internal void Sort(int startIndex, int length, Comparison<T> comparer)
        {
            if ( length < 2 )
                return;

            var firstIndex = OffsetFirstIndex( startIndex );
            ref var firstElementRef = ref GetElementRef( firstIndex );

            for ( var i = (length - 1) >> 1; i >= 0; --i )
                SortFixDown( firstIndex, length, i, comparer );

            while ( length > 1 )
            {
                var lastIndex = Pool.OffsetIndex( firstIndex, --length );
                ref var lastElementRef = ref GetElementRef( lastIndex );
                (firstElementRef, lastElementRef) = (lastElementRef, firstElementRef);
                SortFixDown( firstIndex, length, 0, comparer );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void DeactivateFragmentationIndex()
        {
            Assume.IsGreaterThanOrEqualTo( FragmentationIndex, 0 );
            FragmentationIndex = InactiveFragmentationIndex;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void UpdateFragmentationIndex(int index)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0 );
            FragmentationIndex = index;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SwapFragmentationIndex(Node other)
        {
            Assume.IsGreaterThanOrEqualTo( FragmentationIndex, 0 );
            Assume.IsGreaterThanOrEqualTo( other.FragmentationIndex, 0 );
            (FragmentationIndex, other.FragmentationIndex) = (other.FragmentationIndex, FragmentationIndex);
        }

        internal void Free()
        {
            if ( IsReusable )
                return;

            if ( Next is null )
                FreeAsTail();
            else if ( Prev is null )
                FreeAsHead();
            else
                FreeAsIntermediate();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static Node GetOrCreateNode(MemorySequencePool<T> pool)
        {
            if ( ! pool._nodeCache.TryPop( out var node ) )
                return new Node( pool );

            Assume.Equals( node.FragmentationIndex, CachedFragmentationIndex );
            Assume.Equals( node.Length, 0 );
            Assume.IsNull( node.Prev );
            Assume.IsNull( node.Next );

            node.FragmentationIndex = InactiveFragmentationIndex;
            return node;
        }

        private void FreeAsTail()
        {
            Assume.IsNull( Next );

            if ( Prev is null )
                Pool._tailNode = null;
            else
            {
                if ( ! Prev.IsReusable )
                {
                    Pool._tailNode = Prev;
                    Prev.Next = null;
                }
                else
                {
                    Pool.RemoveFromFragmentationHeap( Prev );
                    Pool._tailNode = Prev.Prev;

                    if ( Prev.Prev is not null )
                    {
                        Assume.False( Prev.Prev.IsReusable );
                        Prev.Prev.Next = null;
                        Prev.Prev = null;
                    }

                    Prev.Next = null;
                    Prev.Deactivate();
                }

                Prev = null;
            }

            Deactivate();
        }

        private void FreeAsHead()
        {
            Assume.IsNotNull( Next );
            Assume.IsNull( Prev );

            if ( ! Next.IsReusable )
            {
                if ( Length > 0 )
                {
                    if ( Pool.ClearReturnedSequences )
                        ClearSegments( FirstIndex, LastIndex );

                    Pool.AddToFragmentationHeap( this );
                    return;
                }

                Next.Prev = null;
            }
            else
            {
                Next.Prev = null;
                Next.PrependFragmentedLength( Length, FirstIndex );
            }

            Next = null;
            Deactivate();
        }

        private void FreeAsIntermediate()
        {
            Assume.IsNotNull( Next );
            Assume.IsNotNull( Prev );

            if ( ! Next.IsReusable )
            {
                if ( ! Prev.IsReusable )
                {
                    if ( Length > 0 )
                    {
                        if ( Pool.ClearReturnedSequences )
                            ClearSegments( FirstIndex, LastIndex );

                        Pool.AddToFragmentationHeap( this );
                        return;
                    }

                    Prev.Next = Next;
                    Next.Prev = Prev;
                }
                else
                {
                    Prev.Next = Next;
                    Next.Prev = Prev;
                    Prev.AppendFragmentedLength( Length, LastIndex );
                }
            }
            else if ( ! Prev.IsReusable )
            {
                Prev.Next = Next;
                Next.Prev = Prev;
                Next.PrependFragmentedLength( Length, FirstIndex );
            }
            else
            {
                Assume.IsNotNull( Next.Next );
                Assume.False( Next.Next.IsReusable );
                Pool.RemoveFromFragmentationHeap( Next );

                Prev.Next = Next.Next;
                Next.Next.Prev = Prev;
                Prev.AppendFragmentedLength( Length + Next.Length, Next.LastIndex );

                Next.Next = null;
                Next.Prev = null;
                Next.Deactivate();
            }

            Next = null;
            Prev = null;
            Deactivate();
        }

        private void ClearSegments(ArraySequenceIndex first, ArraySequenceIndex last)
        {
            if ( first.Segment == last.Segment )
            {
                Array.Clear( GetAbsoluteSegment( first.Segment ), first.Element, last.Element - first.Element + 1 );
                return;
            }

            Array.Clear( GetAbsoluteSegment( first.Segment ), first.Element, Pool.SegmentLength - first.Element );

            for ( var i = first.Segment + 1; i < last.Segment; ++i )
                Array.Clear( GetAbsoluteSegment( i ) );

            Array.Clear( GetAbsoluteSegment( last.Segment ), 0, last.Element + 1 );
        }

        [Pure]
        private int IndexOf(T item, ArraySequenceIndex first, ArraySequenceIndex last)
        {
            int index;
            if ( first.Segment == last.Segment )
            {
                index = Array.IndexOf( GetAbsoluteSegment( first.Segment ), item, first.Element, last.Element - first.Element + 1 );
                return index != -1 ? index - first.Element : index;
            }

            var firstSegmentLength = Pool.SegmentLength - first.Element;
            index = Array.IndexOf( GetAbsoluteSegment( first.Segment ), item, first.Element, firstSegmentLength );
            if ( index != -1 )
                return index - first.Element;

            var secondSegmentIndex = first.Segment + 1;
            for ( var i = secondSegmentIndex; i < last.Segment; ++i )
            {
                index = Array.IndexOf( GetAbsoluteSegment( i ), item );
                if ( index != -1 )
                    return ((i - secondSegmentIndex) << Pool._segmentLengthLog2) + firstSegmentLength + index;
            }

            index = Array.IndexOf( GetAbsoluteSegment( last.Segment ), item, 0, last.Element + 1 );
            return index != -1 ? ((last.Segment - secondSegmentIndex) << Pool._segmentLengthLog2) + firstSegmentLength + index : index;
        }

        private void CopyTo(Span<T> span, ArraySequenceIndex first, ArraySequenceIndex last)
        {
            if ( first.Segment == last.Segment )
            {
                GetAbsoluteSegment( first.Segment ).AsSpan( first.Element, last.Element - first.Element + 1 ).CopyTo( span );
                return;
            }

            GetAbsoluteSegment( first.Segment ).AsSpan( first.Element ).CopyTo( span );
            span = span.Slice( Pool.SegmentLength - first.Element );

            for ( var i = first.Segment + 1; i < last.Segment; ++i )
            {
                GetAbsoluteSegment( i ).CopyTo( span );
                span = span.Slice( Pool.SegmentLength );
            }

            GetAbsoluteSegment( last.Segment ).AsSpan( 0, last.Element + 1 ).CopyTo( span );
        }

        private void CopyFrom(ReadOnlySpan<T> span, ArraySequenceIndex first)
        {
            if ( span.Length == 0 )
                return;

            var segment = GetAbsoluteSegment( first.Segment ).AsSpan( first.Element );
            if ( segment.Length >= span.Length )
            {
                span.CopyTo( segment );
                return;
            }

            span.Slice( 0, segment.Length ).CopyTo( segment );
            span = span.Slice( segment.Length );
            var segmentIndex = first.Segment + 1;
            segment = GetAbsoluteSegment( segmentIndex );

            while ( segment.Length < span.Length )
            {
                span.Slice( 0, segment.Length ).CopyTo( segment );
                span = span.Slice( segment.Length );
                segment = GetAbsoluteSegment( ++segmentIndex );
            }

            span.CopyTo( segment );
        }

        private void SortFixDown(ArraySequenceIndex firstIndex, int length, int parentIndex, Comparison<T> comparer)
        {
            var leftIndex = (parentIndex << 1) + 1;

            while ( leftIndex < length )
            {
                var index = Pool.OffsetIndex( firstIndex, parentIndex );
                ref var parentRef = ref GetElementRef( index );

                var swapIndex = leftIndex;
                index = Pool.OffsetIndex( firstIndex, swapIndex );
                ref var swapRef = ref GetElementRef( index );

                if ( comparer( swapRef, parentRef ) <= 0 )
                {
                    swapIndex = parentIndex;
                    swapRef = ref parentRef;
                }

                var rightIndex = leftIndex + 1;
                if ( rightIndex < length )
                {
                    index = Pool.OffsetIndex( firstIndex, rightIndex );
                    ref var rightRef = ref GetElementRef( index );

                    if ( comparer( rightRef, swapRef ) > 0 )
                    {
                        swapIndex = rightIndex;
                        swapRef = ref rightRef;
                    }
                }

                if ( swapIndex == parentIndex )
                    break;

                (parentRef, swapRef) = (swapRef, parentRef);
                parentIndex = swapIndex;
                leftIndex = (parentIndex << 1) + 1;
            }
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private ref T GetElementRef(ArraySequenceIndex index)
        {
            return ref Unsafe.Add( ref MemoryMarshal.GetArrayDataReference( GetAbsoluteSegment( index.Segment ) ), index.Element );
        }

        private void Deactivate()
        {
            Assume.Equals( FragmentationIndex, InactiveFragmentationIndex );
            Assume.IsNull( Prev );
            Assume.IsNull( Next );

            Pool._nodeCache.Push( this );
            if ( Pool.ClearReturnedSequences )
                ClearSegments();

            FragmentationIndex = CachedFragmentationIndex;
            FirstIndex = ArraySequenceIndex.Zero;
            LastIndex = Pool.GetMinusOneIndex();
            Length = 0;
        }

        private void DecreaseFragmentedLength(int offset)
        {
            Assume.IsInExclusiveRange( offset, 0, Length );
            Assume.IsGreaterThanOrEqualTo( FragmentationIndex, 0 );

            FirstIndex = OffsetFirstIndex( offset );
            Length -= offset;
            AssumeLastIndexValidity();
            Pool.FixDownFragmentationHeap( this );
        }

        private void PrependFragmentedLength(int offset, ArraySequenceIndex firstIndex)
        {
            if ( offset == 0 )
                return;

            Assume.IsGreaterThan( offset, 0 );
            Assume.IsGreaterThanOrEqualTo( FragmentationIndex, 0 );

            Length += offset;
            FirstIndex = firstIndex;
            AssumeLastIndexValidity();
            Pool.FixUpFragmentationHeap( this );
        }

        private void AppendFragmentedLength(int offset, ArraySequenceIndex lastIndex)
        {
            if ( offset == 0 )
                return;

            Assume.IsGreaterThan( offset, 0 );
            Assume.IsGreaterThanOrEqualTo( FragmentationIndex, 0 );

            Length += offset;
            LastIndex = lastIndex;
            AssumeLastIndexValidity();
            Pool.FixUpFragmentationHeap( this );
        }

        [Conditional( "DEBUG" )]
        private void AssumeLastIndexValidity()
        {
            var expected = OffsetFirstIndex( Length - 1 );
            Assume.Equals( LastIndex.Segment, expected.Segment );
            Assume.Equals( LastIndex.Element, expected.Element );
        }
    }
}
