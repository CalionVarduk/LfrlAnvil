using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

public class MemorySequencePool<T>
{
    public readonly struct ReportInfo
    {
        private readonly MemorySequencePool<T>? _pool;

        internal ReportInfo(MemorySequencePool<T> pool)
        {
            _pool = pool;
        }

        public int AllocatedSegments => _pool?._segments.Length ?? 0;
        public int ActiveSegments => (_pool?._tailNode?.LastIndex.Segment ?? -1) + 1;
        public int CachedNodes => _pool?._nodeCache.Length ?? 0;

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

        public int FragmentedNodes => _pool?._fragmentationHeap.Length ?? 0;

        public int ActiveElements
        {
            get
            {
                var node = _pool?._tailNode;
                return node is not null ? (node.LastIndex.Segment << _pool!._segmentLengthLog2) + node.LastIndex.Element + 1 : 0;
            }
        }

        public int FragmentedElements
        {
            get
            {
                var count = 0;
                var fragmentationHeapLength = _pool?._fragmentationHeap.Length ?? 0;
                if ( fragmentationHeapLength == 0 )
                    return count;

                for ( var i = 0; i < fragmentationHeapLength; ++i )
                    count += _pool!._fragmentationHeap.Get( i ).Length;

                return count;
            }
        }

        [Pure]
        public IEnumerable<int> GetFragmentedNodeSizes()
        {
            var fragmentationHeapLength = _pool?._fragmentationHeap.Length ?? 0;
            for ( var i = 0; i < fragmentationHeapLength; ++i )
                yield return _pool!._fragmentationHeap.Get( i ).Length;
        }

        [Pure]
        public IEnumerable<RentedMemorySequence<T>> GetActiveNodes()
        {
            var node = _pool?._tailNode;
            while ( node is not null )
            {
                if ( node.FragmentationIndex != Node.InactiveFragmentationIndex )
                {
                    node = node.Prev;
                    if ( node is null )
                        break;

                    Assume.Equals( node.FragmentationIndex, Node.InactiveFragmentationIndex, nameof( node.FragmentationIndex ) );
                }

                yield return new RentedMemorySequence<T>( node );

                node = node.Prev;
            }
        }
    }

    private const int DefaultBufferSize = 7;

    private readonly int _segmentLengthLog2;
    private Node? _tailNode;
    private Buffer<Node> _nodeCache;
    private Buffer<Node> _fragmentationHeap;
    private Buffer<T[]> _segments;

    public MemorySequencePool(int minSegmentLength)
    {
        Ensure.IsGreaterThan( minSegmentLength, 0, nameof( minSegmentLength ) );
        Ensure.IsLessThanOrEqualTo( minSegmentLength, 1 << 30, nameof( minSegmentLength ) );

        _segmentLengthLog2 = BitOperations.Log2( BitOperations.RoundUpToPowerOf2( unchecked( (uint)minSegmentLength ) ) );
        SegmentLength = 1 << _segmentLengthLog2;
        ClearReturnedSequences = true;

        _tailNode = null;
        _nodeCache = Buffer<Node>.Create();
        _fragmentationHeap = Buffer<Node>.Create();
        _segments = Buffer<T[]>.Create();
    }

    public bool ClearReturnedSequences { get; set; }
    public int SegmentLength { get; }
    public ReportInfo Report => new ReportInfo( this );

    public RentedMemorySequence<T> Rent(int length)
    {
        if ( length <= 0 )
            return RentedMemorySequence<T>.Empty;

        if ( _fragmentationHeap.Length > 0 )
        {
            var largestNode = _fragmentationHeap.Get( 0 );
            if ( largestNode.Length >= length )
                return AllocateAtLargestFragmentedNode( largestNode, length );
        }

        return AllocateAtTail( length );
    }

    public void TrimExcess()
    {
        var cachedTailSegmentCount = _segments.Length;
        if ( _tailNode is not null )
            cachedTailSegmentCount -= _tailNode.LastIndex.Segment + 1;

        _segments = _segments.PopMany( cachedTailSegmentCount ).TrimExcess();
        _fragmentationHeap = _fragmentationHeap.TrimExcess();
        _nodeCache = _nodeCache.Clear().TrimExcess();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ArraySequenceIndex OffsetIndex(ArraySequenceIndex index, int offset)
    {
        return index.Add( offset, _segmentLengthLog2 );
    }

    private RentedMemorySequence<T> AllocateAtTail(int length)
    {
        var node = Node.CreateTailNode( this, length );

        var segmentsToAllocate = node.LastIndex.Segment - _segments.Length + 1;
        for ( var i = 0; i < segmentsToAllocate; ++i )
            _segments = _segments.Push( new T[SegmentLength] );

        return new RentedMemorySequence<T>( node );
    }

    private RentedMemorySequence<T> AllocateAtLargestFragmentedNode(Node node, int length)
    {
        Assume.Equals( node, _fragmentationHeap.Get( 0 ), nameof( node ) );
        Assume.IsLessThanOrEqualTo( length, node.Length, nameof( length ) );

        if ( node.Length == length )
        {
            PopFromFragmentationHeap( node );
            return new RentedMemorySequence<T>( node );
        }

        var extractedNode = node.ExtractNode( length );
        return new RentedMemorySequence<T>( extractedNode );
    }

    private void AddToFragmentationHeap(Node node)
    {
        Assume.Equals( node.FragmentationIndex, Node.InactiveFragmentationIndex, nameof( node.FragmentationIndex ) );
        node.UpdateFragmentationIndex( _fragmentationHeap.Length );
        _fragmentationHeap = _fragmentationHeap.Push( node );
        FixUpFragmentationHeap( node );
    }

    private void PopFromFragmentationHeap(Node node)
    {
        node.DeactivateFragmentationIndex();
        if ( _fragmentationHeap.Length == 1 )
        {
            _fragmentationHeap = _fragmentationHeap.Pop();
            return;
        }

        var last = _fragmentationHeap.Last();
        _fragmentationHeap.Set( 0, last );
        last.UpdateFragmentationIndex( 0 );
        _fragmentationHeap = _fragmentationHeap.Pop();
        FixDownFragmentationHeap( last );
    }

    private void RemoveFromFragmentationHeap(Node node)
    {
        var last = _fragmentationHeap.Last();
        if ( ReferenceEquals( node, last ) )
        {
            node.DeactivateFragmentationIndex();
            _fragmentationHeap = _fragmentationHeap.Pop();
            return;
        }

        _fragmentationHeap.Set( node.FragmentationIndex, last );
        last.UpdateFragmentationIndex( node.FragmentationIndex );
        node.DeactivateFragmentationIndex();
        _fragmentationHeap = _fragmentationHeap.Pop();
        FixRelativeFragmentationHeap( last, node.Length );
    }

    private void FixUpFragmentationHeap(Node node)
    {
        Assume.NotEquals( node.FragmentationIndex, Node.InactiveFragmentationIndex, nameof( node.FragmentationIndex ) );
        var p = (node.FragmentationIndex - 1) >> 1;

        while ( node.FragmentationIndex > 0 )
        {
            var parent = _fragmentationHeap.Get( p );
            if ( node.Length <= parent.Length )
                break;

            _fragmentationHeap.Set( node.FragmentationIndex, parent );
            _fragmentationHeap.Set( parent.FragmentationIndex, node );
            node.SwapFragmentationIndex( parent );
            p = (p - 1) >> 1;
        }
    }

    private void FixDownFragmentationHeap(Node node)
    {
        Assume.NotEquals( node.FragmentationIndex, Node.InactiveFragmentationIndex, nameof( node.FragmentationIndex ) );
        var l = (node.FragmentationIndex << 1) + 1;

        while ( l < _fragmentationHeap.Length )
        {
            var child = _fragmentationHeap.Get( l );
            var nodeToSwap = node.Length < child.Length ? child : node;

            var r = l + 1;
            if ( r < _fragmentationHeap.Length )
            {
                child = _fragmentationHeap.Get( r );
                if ( nodeToSwap.Length < child.Length )
                    nodeToSwap = child;
            }

            if ( ReferenceEquals( node, nodeToSwap ) )
                break;

            _fragmentationHeap.Set( node.FragmentationIndex, nodeToSwap );
            _fragmentationHeap.Set( nodeToSwap.FragmentationIndex, node );
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
        internal const int InactiveFragmentationIndex = -1;

        private Node(MemorySequencePool<T> pool)
        {
            Pool = pool;
            FirstIndex = ArraySequenceIndex.Zero;
            LastIndex = ArraySequenceIndex.MinusOne;
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
        internal bool IsReusable => Length == 0 || FragmentationIndex != InactiveFragmentationIndex;

        [Pure]
        public override string ToString()
        {
            return $"({FirstIndex}) : ({LastIndex}) [{Length}], {nameof( FragmentationIndex )} = {FragmentationIndex}";
        }

        internal static Node CreateTailNode(MemorySequencePool<T> pool, int length)
        {
            var firstIndex = pool._tailNode is null ? ArraySequenceIndex.Zero : pool.OffsetIndex( pool._tailNode.LastIndex, 1 );
            var node = GetOrCreateNode( pool );
            node.UpdateSpanInfo( firstIndex, length );

            if ( pool._tailNode is not null )
            {
                Assume.IsNull( pool._tailNode.Next, nameof( pool._tailNode.Next ) );
                node.Prev = pool._tailNode;
                node.Prev.Next = node;
            }

            pool._tailNode = node;
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
            return Pool._segments.Get( index );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal T GetElement(int index)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Assume.IsLessThan( index, Length, nameof( index ) );

            var sequenceIndex = OffsetFirstIndex( index );
            var segment = GetAbsoluteSegment( sequenceIndex.Segment );
            return segment[sequenceIndex.Element];
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetElement(int index, T value)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            Assume.IsLessThan( index, Length, nameof( index ) );

            var sequenceIndex = OffsetFirstIndex( index );
            var segment = GetAbsoluteSegment( sequenceIndex.Segment );
            segment[sequenceIndex.Element] = value;
        }

        internal Node ExtractNode(int length)
        {
            Assume.NotEquals( FragmentationIndex, InactiveFragmentationIndex, nameof( FragmentationIndex ) );

            var node = GetOrCreateNode( Pool );
            node.UpdateSpanInfo( FirstIndex, length );

            node.Prev = Prev;
            if ( Prev is not null )
                Prev.Next = node;

            Prev = node;
            node.Next = this;

            DecreaseLength( length );
            return node;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearSegments()
        {
            ClearSegments( FirstIndex, LastIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearSegments(int startIndex, int length)
        {
            var first = OffsetFirstIndex( startIndex );
            var last = Pool.OffsetIndex( first, length - 1 );
            ClearSegments( first, last );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int IndexOf(T item)
        {
            return IndexOf( item, FirstIndex, LastIndex );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal int IndexOf(T item, int startIndex, int length)
        {
            var first = OffsetFirstIndex( startIndex );
            var last = Pool.OffsetIndex( first, length - 1 );
            return IndexOf( item, first, last );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CopyTo(Span<T> span)
        {
            CopyTo( span, FirstIndex, LastIndex );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void CopyTo(Span<T> span, int startIndex, int length)
        {
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
            Assume.NotEquals( FragmentationIndex, InactiveFragmentationIndex, nameof( InactiveFragmentationIndex ) );
            FragmentationIndex = InactiveFragmentationIndex;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void UpdateFragmentationIndex(int index)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            FragmentationIndex = index;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SwapFragmentationIndex(Node other)
        {
            Assume.NotEquals( FragmentationIndex, InactiveFragmentationIndex, nameof( FragmentationIndex ) );
            Assume.NotEquals( other.FragmentationIndex, InactiveFragmentationIndex, nameof( other.FragmentationIndex ) );
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
            if ( pool._nodeCache.Length == 0 )
                return new Node( pool );

            var node = pool._nodeCache.Last();
            pool._nodeCache = pool._nodeCache.Pop();

            Assume.Equals( node.FragmentationIndex, InactiveFragmentationIndex, nameof( node.FragmentationIndex ) );
            Assume.Equals( node.Length, 0, nameof( node.Length ) );
            Assume.IsNull( node.Prev, nameof( node.Prev ) );
            Assume.IsNull( node.Next, nameof( node.Next ) );

            return node;
        }

        private void FreeAsTail()
        {
            Assume.IsNull( Next, nameof( Next ) );

            if ( Prev is null )
                Pool._tailNode = null;
            else
            {
                if ( Prev.FragmentationIndex == InactiveFragmentationIndex )
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
                        Assume.Equals( Prev.Prev.FragmentationIndex, InactiveFragmentationIndex, nameof( Prev.Prev.FragmentationIndex ) );
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
            Assume.IsNotNull( Next, nameof( Next ) );
            Assume.IsNull( Prev, nameof( Prev ) );

            if ( Next.FragmentationIndex == InactiveFragmentationIndex )
                Pool.AddToFragmentationHeap( this );
            else
            {
                Next.Prev = null;
                Next.PrependLength( Length, FirstIndex );
                Next = null;
                Deactivate();
            }
        }

        private void FreeAsIntermediate()
        {
            Assume.IsNotNull( Next, nameof( Next ) );
            Assume.IsNotNull( Prev, nameof( Prev ) );

            if ( Next.FragmentationIndex == InactiveFragmentationIndex )
            {
                if ( Prev.FragmentationIndex == InactiveFragmentationIndex )
                {
                    Pool.AddToFragmentationHeap( this );
                    return;
                }

                Prev.Next = Next;
                Next.Prev = Prev;
                Prev.AppendLength( Length, LastIndex );
            }
            else if ( Prev.FragmentationIndex == InactiveFragmentationIndex )
            {
                Prev.Next = Next;
                Next.Prev = Prev;
                Next.PrependLength( Length, FirstIndex );
            }
            else
            {
                Assume.IsNotNull( Next.Next, nameof( Next.Next ) );
                Assume.Equals( Next.Next.FragmentationIndex, InactiveFragmentationIndex, nameof( Next.Next.FragmentationIndex ) );
                Pool.RemoveFromFragmentationHeap( Next );

                Prev.Next = Next.Next;
                Next.Next.Prev = Prev;
                Prev.AppendLength( Length + Next.Length, Next.LastIndex );

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
            Assume.Equals( FragmentationIndex, InactiveFragmentationIndex, nameof( FragmentationIndex ) );
            Assume.IsNull( Prev, nameof( Prev ) );
            Assume.IsNull( Next, nameof( Next ) );

            Pool._nodeCache = Pool._nodeCache.Push( this );
            if ( Pool.ClearReturnedSequences )
                ClearSegments();

            FirstIndex = ArraySequenceIndex.Zero;
            LastIndex = ArraySequenceIndex.MinusOne;
            Length = 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void UpdateSpanInfo(ArraySequenceIndex firstIndex, int length)
        {
            Assume.IsGreaterThan( length, 0, nameof( length ) );
            FirstIndex = firstIndex;
            Length = length;
            LastIndex = OffsetFirstIndex( length - 1 );
        }

        private void DecreaseLength(int offset)
        {
            Assume.IsInExclusiveRange( offset, 0, Length, nameof( offset ) );
            Assume.NotEquals( FragmentationIndex, InactiveFragmentationIndex, nameof( FragmentationIndex ) );

            FirstIndex = OffsetFirstIndex( offset );
            Length -= offset;
            AssumeLastIndexValidity();
            Pool.FixDownFragmentationHeap( this );
        }

        private void PrependLength(int offset, ArraySequenceIndex firstIndex)
        {
            Assume.IsGreaterThan( offset, 0, nameof( offset ) );
            Assume.NotEquals( FragmentationIndex, InactiveFragmentationIndex, nameof( FragmentationIndex ) );

            Length += offset;
            FirstIndex = firstIndex;
            AssumeLastIndexValidity();
            Pool.FixUpFragmentationHeap( this );
        }

        private void AppendLength(int offset, ArraySequenceIndex lastIndex)
        {
            Assume.IsGreaterThan( offset, 0, nameof( offset ) );
            Assume.NotEquals( FragmentationIndex, InactiveFragmentationIndex, nameof( FragmentationIndex ) );

            Length += offset;
            LastIndex = lastIndex;
            AssumeLastIndexValidity();
            Pool.FixUpFragmentationHeap( this );
        }

        [Conditional( "DEBUG" )]
        private void AssumeLastIndexValidity()
        {
            var expected = OffsetFirstIndex( Length - 1 );
            Assume.Equals( LastIndex.Segment, expected.Segment, nameof( LastIndex.Segment ) );
            Assume.Equals( LastIndex.Element, expected.Element, nameof( LastIndex.Element ) );
        }
    }

    private readonly struct Buffer<TElement>
    {
        internal readonly TElement?[] Data;
        internal readonly int Length;

        private Buffer(TElement?[] data, int length)
        {
            Data = data;
            Length = length;
        }

        [Pure]
        internal static Buffer<TElement> Create()
        {
            return new Buffer<TElement>( new TElement?[DefaultBufferSize], 0 );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TElement Get(int index)
        {
            Assume.IsInRange( index, 0, Length - 1, nameof( index ) );
            return Data[index]!;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Set(int index, TElement element)
        {
            Assume.IsInRange( index, 0, Length - 1, nameof( index ) );
            Data[index] = element;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal TElement Last()
        {
            Assume.IsGreaterThan( Length, 0, nameof( Length ) );
            return Data[Length - 1]!;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Buffer<TElement> Pop()
        {
            Assume.IsGreaterThan( Length, 0, nameof( Length ) );
            var nextLength = Length - 1;
            Data[nextLength] = default;
            return new Buffer<TElement>( Data, nextLength );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Buffer<TElement> PopMany(int count)
        {
            Assume.IsGreaterThanOrEqualTo( count, 0, nameof( count ) );
            Assume.IsLessThanOrEqualTo( count, Length, nameof( count ) );

            var length = Length - count;
            if ( length != Length )
                Array.Clear( Data, length, count );

            return new Buffer<TElement>( Data, length );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Buffer<TElement> Push(TElement element)
        {
            var data = Data;
            if ( Length == Data.Length )
                Array.Resize( ref data, checked( (Data.Length << 1) + 1 ) );

            data[Length] = element;
            return new Buffer<TElement>( data, Length + 1 );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Buffer<TElement> Clear()
        {
            if ( Length > 0 )
                Array.Clear( Data, 0, Length );

            return new Buffer<TElement>( Data, 0 );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Buffer<TElement> TrimExcess()
        {
            var capacity = 1U << BitOperations.Log2( BitOperations.RoundUpToPowerOf2( unchecked( (uint)Length ) ) );
            capacity = Math.Max( (capacity == Length ? capacity << 1 : capacity) - 1, DefaultBufferSize );

            var data = Data;
            Array.Resize( ref data, unchecked( (int)capacity ) );
            return new Buffer<TElement>( data, Length );
        }
    }
}
