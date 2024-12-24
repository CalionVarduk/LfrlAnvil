// Copyright 2024 Łukasz Furlepa
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
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Memory;

/// <summary>
/// Represents a pool of <see cref="MemoryPoolToken{T}"/> instances.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public sealed class MemoryPool<T>
{
    /// <summary>
    /// A lightweight <see cref="MemoryPool{T}"/> state report container.
    /// </summary>
    public readonly struct ReportInfo
    {
        private readonly MemoryPool<T>? _pool;

        internal ReportInfo(MemoryPool<T> pool)
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
        public int ActiveSegments
        {
            get
            {
                if ( _pool is null || ! _pool._tailNode.HasValue )
                    return 0;

                ref var tail = ref _pool.GetNodeRef( _pool._tailNode.Value );
                return tail.SegmentIndex + 1;
            }
        }

        /// <summary>
        /// Collection of currently allocated nodes.
        /// </summary>
        public NodeCollection Nodes => new NodeCollection( _pool );

        /// <summary>
        /// Collection of currently allocated fragmented nodes.
        /// </summary>
        public FragmentedNodeCollection FragmentedNodes => new FragmentedNodeCollection( _pool );

        /// <summary>
        /// Represents a collection of fragmented <see cref="MemoryPool{T}"/> nodes.
        /// </summary>
        public readonly struct FragmentedNodeCollection
        {
            private readonly MemoryPool<T>? _pool;

            internal FragmentedNodeCollection(MemoryPool<T>? pool)
            {
                _pool = pool;
            }

            /// <summary>
            /// Number of fragmented underlying pool nodes.
            /// </summary>
            public int Count => _pool?._fragmentationHeap.Count ?? 0;

            /// <summary>
            /// Creates a new <see cref="Enumerator"/> instance for this collection.
            /// </summary>
            /// <returns>New <see cref="Enumerator"/> instance.</returns>
            [Pure]
            public Enumerator GetEnumerator()
            {
                return _pool is null ? default : new Enumerator( _pool );
            }

            /// <summary>
            /// Lightweight enumerator implementation for <see cref="FragmentedNodeCollection"/>.
            /// </summary>
            public struct Enumerator
            {
                private readonly MemoryPool<T>? _pool;
                private int _nextIndex;

                internal Enumerator(MemoryPool<T> pool)
                {
                    _pool = pool;
                    _nextIndex = 0;
                }

                /// <summary>
                /// Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                public FragmentedNode Current { get; private set; }

                /// <summary>
                /// Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
                public bool MoveNext()
                {
                    if ( _nextIndex == (_pool?._fragmentationHeap.Count ?? 0) )
                        return false;

                    Assume.IsNotNull( _pool );
                    var nodeId = _pool.GetFragmentationHeapIdRef( _nextIndex++ );
                    ref var node = ref _pool.GetNodeRef( nodeId );
                    var segment = _pool.GetSegment( node.SegmentIndex );
                    Current = new FragmentedNode( node.SegmentIndex, segment.Length, node.StartIndex, node.Length );
                    return true;
                }
            }
        }

        /// <summary>
        /// Represents a collection of <see cref="MemoryPool{T}"/> nodes.
        /// </summary>
        public readonly struct NodeCollection
        {
            private readonly MemoryPool<T>? _pool;

            internal NodeCollection(MemoryPool<T>? pool)
            {
                _pool = pool;
            }

            /// <summary>
            /// Creates a new <see cref="Enumerator"/> instance for this collection.
            /// </summary>
            /// <returns>New <see cref="Enumerator"/> instance.</returns>
            [Pure]
            public Enumerator GetEnumerator()
            {
                return _pool is null ? default : new Enumerator( _pool );
            }

            /// <summary>
            /// Lightweight enumerator implementation for <see cref="NodeCollection"/>.
            /// </summary>
            public struct Enumerator
            {
                private readonly MemoryPool<T>? _pool;
                private int _nextInactiveTailSegmentIndex;
                private NullableIndex _nextNode;
                private bool _readingNodes;

                internal Enumerator(MemoryPool<T> pool)
                {
                    _pool = pool;
                    _nextInactiveTailSegmentIndex = 0;
                    _nextNode = pool._tailNode;
                    _readingNodes = true;
                }

                /// <summary>
                /// Gets the element in the collection at the current position of the enumerator.
                /// </summary>
                public Node Current { get; private set; }

                /// <summary>
                /// Advances the enumerator to the next element of the collection.
                /// </summary>
                /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
                public bool MoveNext()
                {
                    if ( ! _readingNodes )
                        return MoveNextInactiveTailSegment();

                    if ( ! _nextNode.HasValue )
                    {
                        _readingNodes = false;
                        var result = MoveActiveTailSegmentEnd();
                        return result || MoveNextInactiveTailSegment();
                    }

                    MoveNextNode();
                    return true;
                }

                [MethodImpl( MethodImplOptions.AggressiveInlining )]
                private bool MoveNextInactiveTailSegment()
                {
                    if ( _nextInactiveTailSegmentIndex >= (_pool?._segments.Count ?? 0) )
                        return false;

                    Assume.IsNotNull( _pool );
                    var segment = _pool._segments[_nextInactiveTailSegmentIndex];
                    Current = new Node(
                        pool: _pool,
                        segmentIndex: _nextInactiveTailSegmentIndex++,
                        segmentLength: segment.Length,
                        isSegmentActive: false,
                        nodeId: NullableIndex.Null,
                        startIndex: 0,
                        length: segment.Length,
                        isFragmented: false );

                    return true;
                }

                [MethodImpl( MethodImplOptions.AggressiveInlining )]
                private bool MoveActiveTailSegmentEnd()
                {
                    Assume.IsNotNull( _pool );
                    if ( ! _pool._tailNode.HasValue )
                        return false;

                    ref var tailNode = ref _pool.GetNodeRef( _pool._tailNode.Value );
                    var endIndex = tailNode.EndIndex;
                    var segment = _pool._segments[tailNode.SegmentIndex];
                    _nextInactiveTailSegmentIndex = tailNode.SegmentIndex + 1;

                    if ( endIndex == segment.Length )
                        return false;

                    Current = new Node(
                        pool: _pool,
                        segmentIndex: tailNode.SegmentIndex,
                        segmentLength: segment.Length,
                        isSegmentActive: true,
                        nodeId: NullableIndex.Null,
                        startIndex: endIndex,
                        length: segment.Length - endIndex,
                        isFragmented: false );

                    return true;
                }

                [MethodImpl( MethodImplOptions.AggressiveInlining )]
                private void MoveNextNode()
                {
                    Assume.IsNotNull( _pool );
                    ref var node = ref _pool.GetNodeRef( _nextNode.Value );
                    var segment = _pool.GetSegment( node.SegmentIndex );

                    Current = new Node(
                        pool: _pool,
                        segmentIndex: node.SegmentIndex,
                        segmentLength: segment.Length,
                        isSegmentActive: true,
                        nodeId: _nextNode,
                        startIndex: node.StartIndex,
                        length: node.Length,
                        isFragmented: node.FragmentationIndex.HasValue );

                    _nextNode = node.Prev;
                }
            }
        }

        /// <summary>
        /// Represents a single fragmented <see cref="MemoryPool{T}"/> node.
        /// </summary>
        public readonly struct FragmentedNode
        {
            internal FragmentedNode(int segmentIndex, int segmentLength, int startIndex, int length)
            {
                SegmentIndex = segmentIndex;
                SegmentLength = segmentLength;
                StartIndex = startIndex;
                Length = length;
            }

            /// <summary>
            /// Index of the segment to which this node belongs to.
            /// </summary>
            public int SegmentIndex { get; }

            /// <summary>
            /// Length of the segment to which this node belongs to.
            /// </summary>
            public int SegmentLength { get; }

            /// <summary>
            /// Index of an element inside the segment, which is also the first element of the underlying buffer that belongs to this node.
            /// </summary>
            public int StartIndex { get; }

            /// <summary>
            /// Length of the underlying buffer that belongs to this node.
            /// </summary>
            public int Length { get; }

            /// <summary>
            /// Index of an element inside the segment,
            /// which is also an element one position after the last element of the underlying buffer that belongs to this node.
            /// </summary>
            public int EndIndex => StartIndex + Length;

            /// <summary>
            /// Returns a string representation of this <see cref="FragmentedNode"/> instance.
            /// </summary>
            /// <returns>String representation.</returns>
            [Pure]
            public override string ToString()
            {
                return $"Segment: @{SegmentIndex} (Length: {SegmentLength}), Node: [{StartIndex}:{EndIndex - 1}] ({Length})";
            }
        }

        /// <summary>
        /// Represents a single <see cref="MemoryPool{T}"/> node.
        /// </summary>
        public readonly struct Node
        {
            private readonly MemoryPool<T>? _pool;
            private readonly uint _flags;
            private readonly NullableIndex _nodeId;

            internal Node(
                MemoryPool<T>? pool,
                int segmentIndex,
                int segmentLength,
                bool isSegmentActive,
                NullableIndex nodeId,
                int startIndex,
                int length,
                bool isFragmented)
            {
                _pool = pool;
                _flags = ( uint )((isSegmentActive ? 1 : 0) | (isFragmented ? 2 : 0));
                _nodeId = nodeId;
                SegmentIndex = segmentIndex;
                SegmentLength = segmentLength;
                StartIndex = startIndex;
                Length = length;
            }

            /// <summary>
            /// Index of the segment to which this node belongs to.
            /// </summary>
            public int SegmentIndex { get; }

            /// <summary>
            /// Length of the segment to which this node belongs to.
            /// </summary>
            public int SegmentLength { get; }

            /// <summary>
            /// Index of an element inside the segment, which is also the first element of the underlying buffer that belongs to this node.
            /// </summary>
            public int StartIndex { get; }

            /// <summary>
            /// Length of the underlying buffer that belongs to this node.
            /// </summary>
            public int Length { get; }

            /// <summary>
            /// Specifies whether or not the segment to which this node belongs to is active.
            /// Active segments contain at least one node, which is either fragmented or in use.
            /// </summary>
            public bool IsSegmentActive => (_flags & 1) == 1;

            /// <summary>
            /// Specifies whether or not this node is fragmented.
            /// </summary>
            public bool IsFragmented => (_flags & 2) == 2;

            /// <summary>
            /// Index of an element inside the segment,
            /// which is also an element one position after the last element of the underlying buffer that belongs to this node.
            /// </summary>
            public int EndIndex => StartIndex + Length;

            /// <summary>
            /// Returns a string representation of this <see cref="Node"/> instance.
            /// </summary>
            /// <returns>String representation.</returns>
            [Pure]
            public override string ToString()
            {
                var segmentText = $"Segment: @{SegmentIndex} (Length: {SegmentLength}){(IsSegmentActive ? string.Empty : " (inactive)")}";
                var nodeText = IsSegmentActive
                    ? $", Node: [{StartIndex}:{EndIndex - 1}] ({Length}){(IsFragmented ? " (fragmented)" : string.Empty)}"
                    : string.Empty;

                var tailSegmentText = IsSegmentActive && ! _nodeId.HasValue ? " (free tail)" : string.Empty;
                return $"{segmentText}{nodeText}{tailSegmentText}";
            }

            /// <summary>
            /// Attempts to create a new <see cref="MemoryPoolToken{T}"/> instance from this node.
            /// </summary>
            /// <returns>
            /// New <see cref="MemoryPoolToken{T}"/> instance, or <b>null</b> when segment is not active or node is fragmented.
            /// </returns>
            [Pure]
            public MemoryPoolToken<T>? TryGetToken()
            {
                if ( _pool is null || ! _nodeId.HasValue || IsFragmented )
                    return null;

                Assume.True( IsSegmentActive );
                return new MemoryPoolToken<T>( _pool, _nodeId.Value, clear: false );
            }
        }
    }

    private struct Node
    {
        private uint _flags;
        internal int SegmentIndex;
        internal int Length;
        internal int StartIndex;
        internal NullableIndex Prev;
        internal NullableIndex Next;
        internal int EndIndex => StartIndex + Length;
        internal bool IsUnused => _flags == 0;
        internal bool IsActive => Length != 0;
        internal NullableIndex FragmentationIndex => NullableIndex.CreateUnsafe( unchecked( ( int )_flags & NullableIndex.NullValue ) );

        [Pure]
        public override string ToString()
        {
            if ( IsUnused )
                return "(unused)";

            return IsActive
                ? $"Segment: @{SegmentIndex}, [{StartIndex}:{EndIndex - 1}] ({Length}), Prev: {Prev}, Next: {Next}{(FragmentationIndex.HasValue ? $", Fragmentation: @{FragmentationIndex}" : string.Empty)}"
                : $"(free) Prev: {Prev}, Next: {Next}";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void MakeActive(int segmentIndex, int startIndex, int length)
        {
            Assume.IsGreaterThanOrEqualTo( segmentIndex, 0 );
            Assume.IsGreaterThanOrEqualTo( startIndex, 0 );
            Assume.IsGreaterThan( length, 0 );
            _flags = GetFlags( NullableIndex.NullValue );
            SegmentIndex = segmentIndex;
            StartIndex = startIndex;
            Length = length;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void ClearFragmentationIndex()
        {
            Assume.True( IsActive );
            Assume.True( FragmentationIndex.HasValue );
            _flags = GetFlags( NullableIndex.NullValue );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SetFragmentationIndex(int index)
        {
            Assume.IsInRange( index, 0, NullableIndex.NullValue - 1 );
            Assume.True( IsActive );
            _flags = GetFlags( index );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void MakeFree()
        {
            Assume.True( IsActive );
            _flags = uint.MaxValue;
            SegmentIndex = 0;
            StartIndex = 0;
            Length = 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private static uint GetFlags(int fragmentationIndex)
        {
            return unchecked( ( uint )fragmentationIndex | (NullableIndex.NullValue + 1U) );
        }
    }

    private ListSlim<Node> _nodes;
    private ListSlim<int> _fragmentationHeap;
    private ListSlim<T[]> _segments;
    private NullableIndex _freeListTail;
    private NullableIndex _tailNode;

    /// <summary>
    /// Creates a new <see cref="MemoryPool{T}"/> instance.
    /// </summary>
    /// <param name="minSegmentLength">Minimum single pool segment length. The actual value will be rounded up to a power of two.</param>
    public MemoryPool(int minSegmentLength)
    {
        SegmentLength = Buffers.GetCapacity( minSegmentLength );
        _nodes = ListSlim<Node>.Create();
        _fragmentationHeap = ListSlim<int>.Create();
        _segments = ListSlim<T[]>.Create();
        _freeListTail = _tailNode = NullableIndex.Null;
    }

    /// <summary>
    /// Length of a single pool segment.
    /// </summary>
    public int SegmentLength { get; }

    /// <summary>
    /// Creates a new <see cref="ReportInfo"/> instance.
    /// </summary>
    public ReportInfo Report => new ReportInfo( this );

    /// <summary>
    /// Creates a new <see cref="MemoryPoolToken{T}"/> instance from this pool.
    /// </summary>
    /// <param name="length">Size of the rented buffer.</param>
    /// <returns>
    /// New <see cref="MemoryPoolToken{T}"/> instance,
    /// or <see cref="MemoryPoolToken{T}.Empty"/> when <paramref name="length"/> is less than <b>1</b>.
    /// </returns>
    /// <remarks>This method attempts to reuse fragmented segments.</remarks>
    public MemoryPoolToken<T> Rent(int length)
    {
        if ( length <= 0 )
            return MemoryPoolToken<T>.Empty;

        var id = Allocate( length );
        return new MemoryPoolToken<T>( this, id, clear: false );
    }

    /// <summary>
    /// Creates a new <see cref="MemoryPoolToken{T}"/> instance from this pool.
    /// </summary>
    /// <param name="length">Size of the rented buffer.</param>
    /// <returns>
    /// New <see cref="MemoryPoolToken{T}"/> instance,
    /// or <see cref="MemoryPoolToken{T}.Empty"/> when <paramref name="length"/> is less than <b>1</b>.
    /// </returns>
    /// <remarks>This method always uses or allocates segments at the tail of the pool.</remarks>
    public MemoryPoolToken<T> GreedyRent(int length)
    {
        if ( length <= 0 )
            return MemoryPoolToken<T>.Empty;

        var id = AllocateAtTail( length );
        return new MemoryPoolToken<T>( this, id, clear: false );
    }

    /// <summary>
    /// Attempts to deallocate unused segments at the tail of this pool.
    /// </summary>
    public void TrimExcess()
    {
        if ( _tailNode.HasValue )
        {
            TrimExcessWithTailNode();
            return;
        }

        _nodes.Clear();
        _nodes.ResetCapacity();
        _segments.Clear();
        _segments.ResetCapacity();
        _fragmentationHeap.ResetCapacity();
        _freeListTail = NullableIndex.Null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void Release(int nodeId, bool clear)
    {
        ref var node = ref GetSafeNodeRefOrNull( nodeId );
        if ( Unsafe.IsNullRef( ref node ) )
            return;

        var segment = GetSegment( node.SegmentIndex );
        if ( clear )
            segment.AsSpan( node.StartIndex, node.Length ).Clear();

        if ( ! node.Next.HasValue )
            ReleaseTailNode( nodeId, ref node );
        else if ( ! node.Prev.HasValue )
            ReleaseNonTailHeadNode( nodeId, ref node );
        else
            ReleaseNonTailNonHeadNode( nodeId, ref node );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void SetLength(int nodeId, int length, bool clear)
    {
        Ensure.IsGreaterThan( length, 0 );
        ref var node = ref GetSafeNodeRefOrNull( nodeId );
        if ( Unsafe.IsNullRef( ref node ) || node.Length == length )
            return;

        if ( node.Length > length )
            ReduceLength( nodeId, ref node, length, clear );
        else
            IncreaseLength( nodeId, ref node, length, clear );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Memory<T> AsMemory(int nodeId)
    {
        ref var node = ref GetSafeNodeRefOrNull( nodeId );
        if ( Unsafe.IsNullRef( ref node ) )
            return Memory<T>.Empty;

        var segment = GetSegment( node.SegmentIndex );
        return segment.AsMemory( node.StartIndex, node.Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal Span<T> AsSpan(int nodeId)
    {
        ref var node = ref GetSafeNodeRefOrNull( nodeId );
        if ( Unsafe.IsNullRef( ref node ) )
            return Span<T>.Empty;

        var segment = GetSegment( node.SegmentIndex );
        return segment.AsSpan( node.StartIndex, node.Length );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal ReportInfo.Node? TryGetInfo(int nodeId)
    {
        Assume.IsGreaterThanOrEqualTo( nodeId, 0 );

        ref var node = ref Unsafe.NullRef<Node>();
        if ( nodeId < _nodes.Count )
            node = ref GetNodeRef( nodeId );

        if ( Unsafe.IsNullRef( ref node ) || ! node.IsActive )
            return null;

        var segment = GetSegment( node.SegmentIndex );
        var result = new ReportInfo.Node(
            pool: this,
            segmentIndex: node.SegmentIndex,
            segmentLength: segment.Length,
            isSegmentActive: true,
            nodeId: NullableIndex.CreateUnsafe( nodeId ),
            startIndex: node.StartIndex,
            length: node.Length,
            isFragmented: node.FragmentationIndex.HasValue );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal static string GetLengthString(MemoryPool<T>? pool, int nodeId)
    {
        var length = 0;
        if ( pool is not null )
        {
            ref var node = ref pool.GetSafeNodeRefOrNull( nodeId );
            if ( ! Unsafe.IsNullRef( ref node ) )
                length = node.Length;
        }

        return $"Length: {length}";
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ReduceLength(int nodeId, ref Node node, int length, bool clear)
    {
        Assume.IsLessThan( length, node.Length );
        var segment = GetSegment( node.SegmentIndex );
        var endIndex = node.EndIndex;

        var removed = node.Length - length;
        if ( clear )
            segment.AsSpan( endIndex - removed, removed ).Clear();

        node.Length = length;
        if ( ! node.Next.HasValue )
        {
            Assume.Equals( nodeId, _tailNode.Value );
            return;
        }

        ref var next = ref GetNodeRef( node.Next.Value );
        var nextFragmentationIndex = next.FragmentationIndex;
        if ( node.SegmentIndex == next.SegmentIndex && nextFragmentationIndex.HasValue )
        {
            next.StartIndex -= removed;
            next.Length += removed;
            FixUpFragmentationHeap( nextFragmentationIndex.Value );
        }
        else
        {
            ref var freeNode = ref AddDefaultNode( out var freeId );
            node = ref GetNodeRef( nodeId );
            next = ref GetNodeRef( node.Next.Value );

            var heapIndex = _fragmentationHeap.Count;
            freeNode.MakeActive( node.SegmentIndex, endIndex - removed, removed );
            freeNode.Prev = NullableIndex.Create( nodeId );
            freeNode.Next = node.Next;
            freeNode.SetFragmentationIndex( heapIndex );
            node.Next = NullableIndex.Create( freeId );
            next.Prev = node.Next;
            _fragmentationHeap.Add( freeId );
            FixUpFragmentationHeap( heapIndex );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void IncreaseLength(int nodeId, ref Node node, int length, bool clear)
    {
        Assume.IsGreaterThan( length, node.Length );
        var segment = GetSegment( node.SegmentIndex );
        var endIndex = node.EndIndex;

        var added = length - node.Length;
        if ( ! node.Next.HasValue )
        {
            Assume.Equals( nodeId, _tailNode.Value );
            var remaining = segment.Length - endIndex;
            if ( added <= remaining )
            {
                node.Length += added;
                return;
            }
        }
        else
        {
            var nextId = node.Next.Value;
            ref var next = ref GetNodeRef( nextId );
            var nextFragmentationIndex = next.FragmentationIndex;
            if ( node.SegmentIndex == next.SegmentIndex && nextFragmentationIndex.HasValue )
            {
                if ( added == next.Length )
                {
                    node.Length = length;
                    node.Next = next.Next;
                    ref var nextNext = ref GetNodeRef( next.Next.Value );
                    nextNext.Prev = next.Prev;

                    RemoveFromFragmentationHeap( nextFragmentationIndex.Value, next.Length );
                    DeactivateNode( nextId, ref next );
                    return;
                }

                if ( added < next.Length )
                {
                    node.Length = length;
                    next.StartIndex += added;
                    next.Length -= added;
                    FixDownFragmentationHeap( nextFragmentationIndex.Value );
                    return;
                }
            }
        }

        var id = Allocate( length );
        ref var targetNode = ref GetNodeRef( id );
        node = ref GetNodeRef( nodeId );

        segment = GetSegment( node.SegmentIndex );
        var targetSegment = GetSegment( targetNode.SegmentIndex );
        segment.AsSpan( node.StartIndex, node.Length ).CopyTo( targetSegment.AsSpan( targetNode.StartIndex ) );

        (node, targetNode) = (targetNode, node);

        if ( node.Prev.Value == nodeId )
            node.Prev = NullableIndex.Create( id );
        else if ( node.Prev.HasValue )
        {
            ref var prev = ref GetNodeRef( node.Prev.Value );
            prev.Next = NullableIndex.Create( nodeId );
        }

        if ( node.Next.Value == nodeId )
            node.Next = NullableIndex.Create( id );
        else if ( node.Next.HasValue )
        {
            ref var next = ref GetNodeRef( node.Next.Value );
            next.Prev = NullableIndex.Create( nodeId );
        }

        if ( targetNode.Prev.Value == id )
            targetNode.Prev = NullableIndex.Create( nodeId );
        else if ( targetNode.Prev.HasValue )
        {
            ref var prev = ref GetNodeRef( targetNode.Prev.Value );
            prev.Next = NullableIndex.Create( id );
        }

        if ( targetNode.Next.Value == id )
            targetNode.Next = NullableIndex.Create( nodeId );
        else if ( targetNode.Next.HasValue )
        {
            ref var next = ref GetNodeRef( targetNode.Next.Value );
            next.Prev = NullableIndex.Create( id );
        }

        if ( _tailNode.Value == nodeId )
            _tailNode = NullableIndex.Create( id );
        else if ( _tailNode.Value == id )
            _tailNode = NullableIndex.Create( nodeId );

        Release( id, clear );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ReleaseTailNode(int id, ref Node node)
    {
        Assume.Equals( id, _tailNode.Value );

        var prevId = node.Prev;
        DeactivateNode( id, ref node );

        while ( prevId.HasValue )
        {
            var nodeId = prevId.Value;
            ref var prev = ref GetNodeRef( nodeId );
            prev.Next = NullableIndex.Null;
            var fragmentationIndex = prev.FragmentationIndex;

            if ( ! fragmentationIndex.HasValue )
            {
                _tailNode = prevId;
                return;
            }

            prevId = prev.Prev;
            RemoveFromFragmentationHeap( fragmentationIndex.Value, prev.Length );
            DeactivateNode( nodeId, ref prev );
        }

        _tailNode = NullableIndex.Null;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ReleaseNonTailHeadNode(int id, ref Node node)
    {
        var nextId = node.Next.Value;
        ref var next = ref GetNodeRef( nextId );
        var nextFragmentationIndex = next.FragmentationIndex;

        if ( next.SegmentIndex != node.SegmentIndex || ! nextFragmentationIndex.HasValue )
        {
            var heapIndex = _fragmentationHeap.Count;
            node.SetFragmentationIndex( heapIndex );
            _fragmentationHeap.Add( id );
            FixUpFragmentationHeap( heapIndex );
        }
        else
        {
            next.StartIndex = node.StartIndex;
            next.Length += node.Length;
            next.Prev = NullableIndex.Null;
            DeactivateNode( id, ref node );
            FixUpFragmentationHeap( nextFragmentationIndex.Value );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void ReleaseNonTailNonHeadNode(int id, ref Node node)
    {
        var nextId = node.Next.Value;
        ref var next = ref GetNodeRef( nextId );
        var nextFragmentationIndex = next.FragmentationIndex;

        ref var prev = ref GetNodeRef( node.Prev.Value );
        var prevFragmentationIndex = prev.FragmentationIndex;

        if ( next.SegmentIndex != node.SegmentIndex || ! nextFragmentationIndex.HasValue )
        {
            if ( prev.SegmentIndex != node.SegmentIndex || ! prevFragmentationIndex.HasValue )
            {
                var heapIndex = _fragmentationHeap.Count;
                node.SetFragmentationIndex( heapIndex );
                _fragmentationHeap.Add( id );
                FixUpFragmentationHeap( heapIndex );
            }
            else
            {
                prev.Length += node.Length;
                prev.Next = node.Next;
                next.Prev = node.Prev;
                DeactivateNode( id, ref node );
                FixUpFragmentationHeap( prevFragmentationIndex.Value );
            }
        }
        else
        {
            Assume.NotEquals( next.Next, NullableIndex.Null );

            if ( prev.SegmentIndex != node.SegmentIndex || ! prevFragmentationIndex.HasValue )
            {
                next.StartIndex = node.StartIndex;
                next.Length += node.Length;
                next.Prev = node.Prev;
                prev.Next = node.Next;
                DeactivateNode( id, ref node );
                FixUpFragmentationHeap( nextFragmentationIndex.Value );
            }
            else
            {
                RemoveFromFragmentationHeap( nextFragmentationIndex.Value, next.Length );

                prev.Length += node.Length + next.Length;
                prev.Next = next.Next;
                ref var nextNext = ref GetNodeRef( next.Next.Value );
                nextNext.Prev = node.Prev;

                DeactivateNode( nextId, ref next );
                DeactivateNode( id, ref node );
                FixUpFragmentationHeap( prevFragmentationIndex.Value );
            }
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void TrimExcessWithTailNode()
    {
        ref var tail = ref GetNodeRef( _tailNode.Value );
        _segments.RemoveLastRange( _segments.Count - tail.SegmentIndex - 1 );

        _segments.ResetCapacity();
        _fragmentationHeap.ResetCapacity();

        if ( _freeListTail.HasValue )
        {
            Assume.False( _nodes.IsEmpty );

            var index = _nodes.Count - 1;
            ref var node = ref GetNodeRef( index );
            while ( ! node.IsActive )
            {
                --index;
                node = ref Unsafe.Subtract( ref node, 1 );
            }

            Assume.IsGreaterThanOrEqualTo( index, 0 );
            if ( _nodes.RemoveLastRange( _nodes.Count - index - 1 ) > 0 )
            {
                --index;
                node = ref Unsafe.Subtract( ref node, 1 );
                while ( index >= 0 && node.IsActive )
                {
                    --index;
                    node = ref Unsafe.Subtract( ref node, 1 );
                }

                if ( index >= 0 )
                {
                    Assume.False( node.IsActive );
                    _freeListTail = NullableIndex.Create( index );
                    node.Prev = NullableIndex.Null;
                    node.Next = NullableIndex.Null;

                    --index;
                    node = ref Unsafe.Subtract( ref node, 1 );
                    while ( index >= 0 )
                    {
                        if ( ! node.IsActive )
                        {
                            tail = ref GetNodeRef( _freeListTail.Value );
                            node.Prev = _freeListTail;
                            node.Next = NullableIndex.Null;
                            tail.Next = NullableIndex.Create( index );
                            _freeListTail = tail.Next;
                        }

                        node = ref Unsafe.Subtract( ref node, 1 );
                        --index;
                    }
                }
                else
                    _freeListTail = NullableIndex.Null;
            }
        }

        _nodes.ResetCapacity();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int Allocate(int length)
    {
        Assume.IsGreaterThan( length, 0 );

        if ( ! _fragmentationHeap.IsEmpty )
        {
            var id = _fragmentationHeap.First();
            ref var node = ref GetNodeRef( id );

            if ( node.Length > length )
            {
                PartiallyAllocateAtLargestFragmentedNode( id, length );
                return id;
            }

            if ( node.Length == length )
            {
                AllocateAtLargestFragmentedNode( ref node );
                return id;
            }
        }

        return AllocateAtTail( length );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void PartiallyAllocateAtLargestFragmentedNode(int id, int length)
    {
        ref var freeNode = ref AddDefaultNode( out var freeId );
        ref var node = ref GetNodeRef( id );
        Assume.IsInRange( length, 1, node.Length - 1 );

        freeNode.MakeActive( node.SegmentIndex, node.StartIndex + length, node.Length - length );
        freeNode.Prev = NullableIndex.Create( id );
        freeNode.Next = node.Next;
        freeNode.SetFragmentationIndex( 0 );

        node.Length = length;
        node.Next = NullableIndex.Create( freeId );
        node.ClearFragmentationIndex();

        ref var next = ref GetNodeRef( freeNode.Next.Value );
        next.Prev = node.Next;

        ref var firstId = ref _fragmentationHeap.First();
        firstId = freeId;
        FixDownFragmentationHeap( 0 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AllocateAtLargestFragmentedNode(ref Node node)
    {
        Assume.False( _fragmentationHeap.IsEmpty );

        node.ClearFragmentationIndex();
        if ( _fragmentationHeap.Count == 1 )
        {
            _fragmentationHeap.RemoveLast();
            return;
        }

        ref var firstId = ref _fragmentationHeap.First();
        var lastId = Unsafe.Add( ref firstId, _fragmentationHeap.Count - 1 );

        ref var last = ref GetNodeRef( lastId );
        last.SetFragmentationIndex( 0 );

        firstId = lastId;
        _fragmentationHeap.RemoveLast();
        FixDownFragmentationHeap( 0 );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T[] GetSegment(int index)
    {
        Assume.IsInRange( index, 0, _segments.Count - 1 );
        ref var first = ref _segments.First();
        return Unsafe.Add( ref first, index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int AllocateFirstNodeAtTail(int length)
    {
        int id;
        if ( length <= SegmentLength )
        {
            if ( _segments.IsEmpty )
            {
                var segment = new T[SegmentLength];
                _segments.Add( segment );
            }

            ref var node = ref AddDefaultNode( out id );
            node.MakeActive( 0, 0, length );
            node.Prev = NullableIndex.Null;
            node.Next = node.Prev;
        }
        else
        {
            var segmentIndex = FindFittingLargeTailSegmentIndex( -1, length );
            ref var node = ref AddDefaultNode( out id );
            node.MakeActive( segmentIndex, 0, length );
            node.Prev = _tailNode;
            node.Next = NullableIndex.Null;

            if ( _tailNode.HasValue )
            {
                ref var tailNode = ref GetNodeRef( _tailNode.Value );
                tailNode.Next = NullableIndex.Create( id );
            }
        }

        return id;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int AllocateNextNodeAtTail(int length)
    {
        int id;
        ref var tail = ref GetNodeRef( _tailNode.Value );
        var tailSegment = GetSegment( tail.SegmentIndex );
        var endIndex = tail.EndIndex;
        var remaining = tailSegment.Length - endIndex;

        if ( length <= remaining )
        {
            ref var node = ref AddDefaultNode( out id );
            tail = ref GetNodeRef( _tailNode.Value );

            node.MakeActive( tail.SegmentIndex, endIndex, length );
            node.Prev = _tailNode;
            node.Next = NullableIndex.Null;
            tail.Next = NullableIndex.Create( id );
        }
        else if ( length <= SegmentLength )
        {
            var segmentIndex = tail.SegmentIndex + 1;
            if ( _segments.Count <= segmentIndex )
            {
                var segment = new T[SegmentLength];
                _segments.Add( segment );
            }

            ref var node = ref AddDefaultNode( out id );
            node.MakeActive( segmentIndex, 0, length );
            node.Next = NullableIndex.Null;

            if ( remaining > 0 )
            {
                var heapIndex = _fragmentationHeap.Count;
                ref var freeNode = ref AddDefaultNode( out var freeId );
                tail = ref GetNodeRef( _tailNode.Value );
                node = ref GetNodeRef( id );

                freeNode.MakeActive( tail.SegmentIndex, endIndex, remaining );
                freeNode.Prev = _tailNode;
                freeNode.Next = NullableIndex.Create( id );
                node.Prev = NullableIndex.Create( freeId );
                tail.Next = node.Prev;

                freeNode.SetFragmentationIndex( heapIndex );
                _fragmentationHeap.Add( freeId );
                FixUpFragmentationHeap( heapIndex );
            }
            else
            {
                tail = ref GetNodeRef( _tailNode.Value );
                tail.Next = NullableIndex.Create( id );
                node.Prev = _tailNode;
            }
        }
        else
        {
            var oldTailId = _tailNode;
            var segmentIndex = FindFittingLargeTailSegmentIndex( tail.SegmentIndex, length );
            ref var node = ref AddDefaultNode( out id );
            node.MakeActive( segmentIndex, 0, length );
            node.Next = NullableIndex.Null;

            if ( remaining > 0 )
            {
                var heapIndex = _fragmentationHeap.Count;
                ref var freeNode = ref AddDefaultNode( out var freeId );
                tail = ref GetNodeRef( _tailNode.Value );
                node = ref GetNodeRef( id );

                if ( oldTailId.Value != _tailNode.Value )
                {
                    ref var oldTail = ref GetNodeRef( oldTailId.Value );
                    freeNode.MakeActive( oldTail.SegmentIndex, endIndex, remaining );
                    freeNode.Prev = oldTailId;
                    freeNode.Next = oldTail.Next;

                    ref var next = ref GetNodeRef( oldTail.Next.Value );
                    next.Prev = NullableIndex.Create( freeId );
                    oldTail.Next = next.Prev;
                    node.Prev = _tailNode;
                    tail.Next = NullableIndex.Create( id );
                }
                else
                {
                    freeNode.MakeActive( tail.SegmentIndex, endIndex, remaining );
                    freeNode.Prev = _tailNode;
                    freeNode.Next = NullableIndex.Create( id );
                    node.Prev = NullableIndex.Create( freeId );
                    tail.Next = node.Prev;
                }

                freeNode.SetFragmentationIndex( heapIndex );
                _fragmentationHeap.Add( freeId );
                FixUpFragmentationHeap( heapIndex );
            }
            else
            {
                tail = ref GetNodeRef( _tailNode.Value );
                tail.Next = NullableIndex.Create( id );
                node.Prev = _tailNode;
            }
        }

        return id;
    }

    private int AllocateAtTail(int length)
    {
        Assume.IsGreaterThan( length, 0 );
        var id = _tailNode.HasValue ? AllocateNextNodeAtTail( length ) : AllocateFirstNodeAtTail( length );
        _tailNode = NullableIndex.Create( id );
        return id;
    }

    private int FindFittingLargeTailSegmentIndex(int tailIndex, int length)
    {
        Assume.IsGreaterThan( length, SegmentLength );

        var index = tailIndex + 1;
        while ( index < _segments.Count )
        {
            var segment = GetSegment( index );
            if ( length <= segment.Length )
                return index;

            var heapIndex = _fragmentationHeap.Count;
            ref var node = ref AddDefaultNode( out var id );
            node.MakeActive( index, 0, segment.Length );
            node.Next = NullableIndex.Null;
            node.SetFragmentationIndex( heapIndex );

            if ( ! _tailNode.HasValue )
            {
                Assume.True( _fragmentationHeap.IsEmpty );
                node.Prev = NullableIndex.Null;
                _fragmentationHeap.Add( id );
            }
            else
            {
                ref var prev = ref GetNodeRef( _tailNode.Value );
                prev.Next = NullableIndex.Create( id );
                node.Prev = _tailNode;
                _fragmentationHeap.Add( id );
                FixUpFragmentationHeap( heapIndex );
            }

            _tailNode = NullableIndex.Create( id );
            ++index;
        }

        var newSegment = new T[Buffers.GetCapacity( length )];
        _segments.Add( newSegment );
        return index;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref Node GetNodeRef(int index)
    {
        Assume.IsInRange( index, 0, _nodes.Count - 1 );
        ref var first = ref _nodes.First();
        ref var node = ref Unsafe.Add( ref first, index );
        return ref node;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref Node GetSafeNodeRefOrNull(int index)
    {
        Assume.IsGreaterThanOrEqualTo( index, 0 );
        if ( index < _nodes.Count )
        {
            ref var node = ref GetNodeRef( index );
            if ( node.IsActive && ! node.FragmentationIndex.HasValue )
                return ref node;
        }

        return ref Unsafe.NullRef<Node>();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref int GetFragmentationHeapIdRef(int index)
    {
        Assume.IsInRange( index, 0, _fragmentationHeap.Count - 1 );
        ref var first = ref _fragmentationHeap.First();
        return ref Unsafe.Add( ref first, index );
    }

    private ref Node AddDefaultNode(out int index)
    {
        ref var result = ref Unsafe.NullRef<Node>();
        if ( ! _freeListTail.HasValue )
        {
            index = _nodes.Count;
            _nodes.Add( default );
            result = ref GetNodeRef( index );
        }
        else
        {
            index = _freeListTail.Value;
            result = ref GetNodeRef( index );

            if ( ! result.Prev.HasValue )
                _freeListTail = NullableIndex.Null;
            else
            {
                ref var entry = ref GetNodeRef( result.Prev.Value );
                Assume.False( entry.IsActive );
                Assume.False( entry.IsUnused );
                Assume.Equals( entry.Next, _freeListTail );
                entry.Next = NullableIndex.Null;
                _freeListTail = result.Prev;
                result.Prev = NullableIndex.Null;
            }
        }

        return ref result;
    }

    private void DeactivateNode(int index, ref Node node)
    {
        Assume.True( node.IsActive );
        node.MakeFree();

        if ( ! _freeListTail.HasValue )
        {
            node.Prev = node.Next = NullableIndex.Null;
            _freeListTail = NullableIndex.Create( index );
        }
        else
        {
            ref var tail = ref GetNodeRef( _freeListTail.Value );
            Assume.False( tail.IsActive );
            Assume.False( tail.IsUnused );
            Assume.False( tail.Next.HasValue );
            tail.Next = NullableIndex.Create( index );
            node.Prev = _freeListTail;
            node.Next = NullableIndex.Null;
            _freeListTail = tail.Next;
        }
    }

    private void FixUpFragmentationHeap(int i)
    {
        var p = (i - 1) >> 1;
        ref var firstId = ref _fragmentationHeap.First();

        while ( i > 0 )
        {
            ref var id = ref Unsafe.Add( ref firstId, i );
            ref var parentId = ref Unsafe.Add( ref firstId, p );
            ref var node = ref GetNodeRef( id );
            ref var parent = ref GetNodeRef( parentId );

            if ( node.Length <= parent.Length )
                break;

            var index = node.FragmentationIndex;
            node.SetFragmentationIndex( parent.FragmentationIndex.Value );
            parent.SetFragmentationIndex( index.Value );

            (id, parentId) = (parentId, id);
            i = p;
            p = (p - 1) >> 1;
        }
    }

    private void FixDownFragmentationHeap(int i)
    {
        var l = (i << 1) + 1;
        ref var firstId = ref _fragmentationHeap.First();

        while ( l < _fragmentationHeap.Count )
        {
            ref var id = ref Unsafe.Add( ref firstId, i );
            ref var childId = ref Unsafe.Add( ref firstId, l );
            ref var node = ref GetNodeRef( id );
            ref var child = ref GetNodeRef( childId );

            var t = i;
            ref var targetId = ref id;
            ref var target = ref node;
            if ( node.Length < child.Length )
            {
                t = l;
                targetId = ref childId;
                target = ref child;
            }

            var r = l + 1;
            if ( r < _fragmentationHeap.Count )
            {
                childId = ref Unsafe.Add( ref firstId, r );
                child = ref GetNodeRef( childId );
                if ( target.Length < child.Length )
                {
                    t = r;
                    targetId = ref childId;
                    target = ref child;
                }
            }

            if ( i == t )
                break;

            var index = node.FragmentationIndex;
            node.SetFragmentationIndex( target.FragmentationIndex.Value );
            target.SetFragmentationIndex( index.Value );

            (id, targetId) = (targetId, id);
            i = t;
            l = (i << 1) + 1;
        }
    }

    private void RemoveFromFragmentationHeap(int heapIndex, int length)
    {
        Assume.IsInRange( heapIndex, 0, _fragmentationHeap.Count - 1 );

        var lastIndex = _fragmentationHeap.Count - 1;
        if ( heapIndex == lastIndex )
            _fragmentationHeap.RemoveLast();
        else
        {
            ref var firstId = ref _fragmentationHeap.First();
            ref var nodeId = ref Unsafe.Add( ref firstId, heapIndex );
            var lastId = Unsafe.Add( ref firstId, lastIndex );
            nodeId = lastId;
            _fragmentationHeap.RemoveLast();

            ref var last = ref GetNodeRef( lastId );
            last.SetFragmentationIndex( heapIndex );

            if ( last.Length < length )
                FixDownFragmentationHeap( heapIndex );
            else
                FixUpFragmentationHeap( heapIndex );
        }
    }
}
