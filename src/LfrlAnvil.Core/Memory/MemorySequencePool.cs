using System;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

public class MemorySequencePool<T>
{
    private const int DefaultBufferSize = 7;

    private readonly int _segmentLengthLog2;
    private Node? _tailNode;
    private Buffer<Node> _nodeCache;
    private Buffer<Node> _freeSequencesHeap;
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
        _freeSequencesHeap = Buffer<Node>.Create();
        _segments = Buffer<T[]>.Create();
    }

    public bool ClearReturnedSequences { get; set; }
    public int SegmentLength { get; }

    public RentedMemorySequence<T> Rent(int length)
    {
        if ( length <= 0 )
            return RentedMemorySequence<T>.Empty;

        if ( _freeSequencesHeap.Length > 0 )
        {
            var largestSequence = _freeSequencesHeap.Get( 0 );
            if ( largestSequence.Length >= length )
                return AllocateAtLargestFreeNode( largestSequence, length );
        }

        return AllocateAtTail( length );
    }

    public void TrimExcess()
    {
        var cachedTailSegmentCount = _segments.Length;
        if ( _tailNode is not null )
            cachedTailSegmentCount -= _tailNode.LastIndex.Segment + 1;

        _segments = _segments.PopMany( cachedTailSegmentCount ).TrimExcess();
        _freeSequencesHeap = _freeSequencesHeap.TrimExcess();
        _nodeCache = _nodeCache.Clear().TrimExcess();
    }

    private RentedMemorySequence<T> AllocateAtTail(int length)
    {
        var node = Node.CreateTailNode( this, length );

        var segmentsToAllocate = node.LastIndex.Segment - _segments.Length + 1;
        for ( var i = 0; i < segmentsToAllocate; ++i )
            _segments = _segments.Push( new T[SegmentLength] );

        return new RentedMemorySequence<T>( node );
    }

    private RentedMemorySequence<T> AllocateAtLargestFreeNode(Node node, int length)
    {
        Assume.Equals( node, _freeSequencesHeap.Get( 0 ), nameof( node ) );
        Assume.IsLessThanOrEqualTo( length, node.Length, nameof( length ) );

        if ( node.Length == length )
        {
            PopFromFreeSequencesHeap( node );
            return new RentedMemorySequence<T>( node );
        }

        var extractedNode = node.ExtractNode( length );
        return new RentedMemorySequence<T>( extractedNode );
    }

    private void AddToFreeSequencesHeap(Node node)
    {
        Assume.Equals( node.FreeHeapIndex, Node.InactiveFreeHeapIndex, nameof( node.FreeHeapIndex ) );
        node.UpdateFreeHeapIndex( _freeSequencesHeap.Length );
        _freeSequencesHeap = _freeSequencesHeap.Push( node );
        FixUpFreeSequencesHeap( node );
    }

    private void PopFromFreeSequencesHeap(Node node)
    {
        node.DeactivateFreeHeapIndex();
        if ( _freeSequencesHeap.Length == 1 )
        {
            _freeSequencesHeap = _freeSequencesHeap.Pop();
            return;
        }

        var last = _freeSequencesHeap.Last();
        _freeSequencesHeap.Set( 0, last );
        last.UpdateFreeHeapIndex( 0 );
        _freeSequencesHeap = _freeSequencesHeap.Pop();
        FixDownFreeSequencesHeap( last );
    }

    private void RemoveFromFreeSequencesHeap(Node node)
    {
        var last = _freeSequencesHeap.Last();
        if ( ReferenceEquals( node, last ) )
        {
            node.DeactivateFreeHeapIndex();
            _freeSequencesHeap = _freeSequencesHeap.Pop();
            return;
        }

        _freeSequencesHeap.Set( node.FreeHeapIndex, last );
        last.UpdateFreeHeapIndex( node.FreeHeapIndex );
        node.DeactivateFreeHeapIndex();
        _freeSequencesHeap = _freeSequencesHeap.Pop();
        FixRelativeFreeSegmentsHeap( last, node.Length );
    }

    private void FixUpFreeSequencesHeap(Node node)
    {
        Assume.NotEquals( node.FreeHeapIndex, Node.InactiveFreeHeapIndex, nameof( node.FreeHeapIndex ) );
        var p = (node.FreeHeapIndex - 1) >> 1;

        while ( node.FreeHeapIndex > 0 )
        {
            var parent = _freeSequencesHeap.Get( p );
            if ( node.Length <= parent.Length )
                break;

            _freeSequencesHeap.Set( node.FreeHeapIndex, parent );
            _freeSequencesHeap.Set( parent.FreeHeapIndex, node );
            node.SwapFreeHeapIndex( parent );
            p = (p - 1) >> 1;
        }
    }

    private void FixDownFreeSequencesHeap(Node node)
    {
        Assume.NotEquals( node.FreeHeapIndex, Node.InactiveFreeHeapIndex, nameof( node.FreeHeapIndex ) );
        var l = (node.FreeHeapIndex << 1) + 1;

        while ( l < _freeSequencesHeap.Length )
        {
            var child = _freeSequencesHeap.Get( l );
            var nodeToSwap = node.Length < child.Length ? child : node;

            var r = l + 1;
            if ( r < _freeSequencesHeap.Length )
            {
                child = _freeSequencesHeap.Get( r );
                if ( nodeToSwap.Length < child.Length )
                    nodeToSwap = child;
            }

            if ( ReferenceEquals( node, nodeToSwap ) )
                break;

            _freeSequencesHeap.Set( node.FreeHeapIndex, nodeToSwap );
            _freeSequencesHeap.Set( nodeToSwap.FreeHeapIndex, node );
            node.SwapFreeHeapIndex( nodeToSwap );
            l = (node.FreeHeapIndex << 1) + 1;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixRelativeFreeSegmentsHeap(Node node, int oldLength)
    {
        if ( node.Length < oldLength )
            FixDownFreeSequencesHeap( node );
        else
            FixUpFreeSequencesHeap( node );
    }

    internal sealed class Node
    {
        internal const int InactiveFreeHeapIndex = -1;

        private Node(MemorySequencePool<T> pool)
        {
            Pool = pool;
            FirstIndex = ArraySequenceIndex.Zero;
            LastIndex = ArraySequenceIndex.MinusOne;
            Length = 0;
            FreeHeapIndex = InactiveFreeHeapIndex;
            Prev = null;
            Next = null;
        }

        internal MemorySequencePool<T> Pool { get; }
        internal ArraySequenceIndex FirstIndex { get; private set; }
        internal ArraySequenceIndex LastIndex { get; private set; }
        internal int Length { get; private set; }
        internal int FreeHeapIndex { get; private set; }
        internal Node? Prev { get; private set; }
        internal Node? Next { get; private set; }
        internal bool IsFree => Length == 0 || FreeHeapIndex != InactiveFreeHeapIndex;

        [Pure]
        public override string ToString()
        {
            return $"({FirstIndex}) : ({LastIndex}) [{Length}], {nameof( FreeHeapIndex )} = {FreeHeapIndex}";
        }

        internal static Node CreateTailNode(MemorySequencePool<T> pool, int length)
        {
            var firstIndex = pool._tailNode?.LastIndex.Increment( pool._segmentLengthLog2 ) ?? ArraySequenceIndex.Zero;
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
            return FirstIndex.Add( offset, Pool._segmentLengthLog2 );
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
            Assume.NotEquals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( FreeHeapIndex ) );

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
            var last = first.Add( length - 1, Pool._segmentLengthLog2 );
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
            var last = first.Add( length - 1, Pool._segmentLengthLog2 );
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
            var last = first.Add( length - 1, Pool._segmentLengthLog2 );
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
        internal void DeactivateFreeHeapIndex()
        {
            Assume.NotEquals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( InactiveFreeHeapIndex ) );
            FreeHeapIndex = InactiveFreeHeapIndex;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void UpdateFreeHeapIndex(int index)
        {
            Assume.IsGreaterThanOrEqualTo( index, 0, nameof( index ) );
            FreeHeapIndex = index;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SwapFreeHeapIndex(Node other)
        {
            Assume.NotEquals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( FreeHeapIndex ) );
            Assume.NotEquals( other.FreeHeapIndex, InactiveFreeHeapIndex, nameof( other.FreeHeapIndex ) );
            (FreeHeapIndex, other.FreeHeapIndex) = (other.FreeHeapIndex, FreeHeapIndex);
        }

        internal void Free()
        {
            if ( IsFree )
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

            Assume.Equals( node.FreeHeapIndex, InactiveFreeHeapIndex, nameof( node.FreeHeapIndex ) );
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
                if ( Prev.FreeHeapIndex == InactiveFreeHeapIndex )
                {
                    Pool._tailNode = Prev;
                    Prev.Next = null;
                }
                else
                {
                    Pool.RemoveFromFreeSequencesHeap( Prev );
                    Pool._tailNode = Prev.Prev;

                    if ( Prev.Prev is not null )
                    {
                        Assume.Equals( Prev.Prev.FreeHeapIndex, InactiveFreeHeapIndex, nameof( Prev.Prev.FreeHeapIndex ) );
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

            if ( Next.FreeHeapIndex == InactiveFreeHeapIndex )
                Pool.AddToFreeSequencesHeap( this );
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

            if ( Next.FreeHeapIndex == InactiveFreeHeapIndex )
            {
                if ( Prev.FreeHeapIndex == InactiveFreeHeapIndex )
                {
                    Pool.AddToFreeSequencesHeap( this );
                    return;
                }

                Prev.Next = Next;
                Next.Prev = Prev;
                Prev.AppendLength( Length, LastIndex );
            }
            else if ( Prev.FreeHeapIndex == InactiveFreeHeapIndex )
            {
                Prev.Next = Next;
                Next.Prev = Prev;
                Next.PrependLength( Length, FirstIndex );
            }
            else
            {
                Assume.IsNotNull( Next.Next, nameof( Next.Next ) );
                Assume.Equals( Next.Next.FreeHeapIndex, InactiveFreeHeapIndex, nameof( Next.Next.FreeHeapIndex ) );
                Pool.RemoveFromFreeSequencesHeap( Next );

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

        private void Deactivate()
        {
            Assume.Equals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( FreeHeapIndex ) );
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
            Assume.NotEquals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( FreeHeapIndex ) );

            FirstIndex = OffsetFirstIndex( offset );
            Length -= offset;
            AssumeLastIndexValidity();
            Pool.FixDownFreeSequencesHeap( this );
        }

        private void PrependLength(int offset, ArraySequenceIndex firstIndex)
        {
            Assume.IsGreaterThan( offset, 0, nameof( offset ) );
            Assume.NotEquals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( FreeHeapIndex ) );

            Length += offset;
            FirstIndex = firstIndex;
            AssumeLastIndexValidity();
            Pool.FixUpFreeSequencesHeap( this );
        }

        private void AppendLength(int offset, ArraySequenceIndex lastIndex)
        {
            Assume.IsGreaterThan( offset, 0, nameof( offset ) );
            Assume.NotEquals( FreeHeapIndex, InactiveFreeHeapIndex, nameof( FreeHeapIndex ) );

            Length += offset;
            LastIndex = lastIndex;
            AssumeLastIndexValidity();
            Pool.FixUpFreeSequencesHeap( this );
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
