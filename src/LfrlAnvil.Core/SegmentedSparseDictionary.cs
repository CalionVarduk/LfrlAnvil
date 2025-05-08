// Copyright 2025 Łukasz Furlepa
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
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil;

/// <summary>
/// Represents a segmented collection of keys and values, where keys are of <see cref="int"/> type, greater than or equal to <b>0</b>.
/// </summary>
/// <typeparam name="T">Value type.</typeparam>
public struct SegmentedSparseDictionary<T>
{
    private Segment[] _segments;
    private readonly int _segmentLengthLog2;

    private SegmentedSparseDictionary(int minSegmentLength)
    {
        _segments = Array.Empty<Segment>();
        SegmentLength = Math.Max( Buffers.GetCapacity( minSegmentLength ), 8 );
        _segmentLengthLog2 = BitOperations.Log2( unchecked( ( uint )SegmentLength ) );
        Count = 0;
    }

    /// <summary>
    /// Specifies the length of each segment.
    /// </summary>
    public int SegmentLength { get; }

    /// <summary>
    /// Gets the number of entries contained in this dictionary.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Gets the number of segments contained in this dictionary.
    /// </summary>
    public int SegmentCount => _segments.Length;

    /// <summary>
    /// Specifies whether or not this dictionary is empty.
    /// </summary>
    public bool IsEmpty => Count == 0;

    /// <summary>
    /// Creates a new empty <see cref="SegmentedSparseDictionary{T}"/> instance.
    /// </summary>
    /// <param name="minSegmentLength">
    /// Minimum length of each segment. Actual <see cref="SegmentLength"/> will be rounded up to the nearest power of two.
    /// </param>
    /// <returns>New <see cref="SegmentedSparseDictionary{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SegmentedSparseDictionary<T> Create(int minSegmentLength)
    {
        return new SegmentedSparseDictionary<T>( minSegmentLength );
    }

    /// <summary>
    /// Checks whether or not an entry with the provided <paramref name="key"/> exists.
    /// </summary>
    /// <param name="key">Entry's key to check.</param>
    /// <returns><b>true</b> when entry exists, otherwise <b>false</b>.</returns>
    [Pure]
    public bool ContainsKey(int key)
    {
        if ( key < 0 )
            return false;

        var segmentIndex = key >> _segmentLengthLog2;
        if ( segmentIndex >= _segments.Length )
            return false;

        ref var segment = ref GetSegmentRef( segmentIndex );
        return segment.IsInitialized && segment.Contains( key & (SegmentLength - 1) );
    }

    /// <summary>
    /// Attempts to get a value associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key of a value to get.</param>
    /// <param name="result">An <b>out</b> parameter that returns a value associated with the <paramref name="key"/>, if it exists.</param>
    /// <returns><b>true</b> when entry exists, otherwise <b>false</b>.</returns>
    public bool TryGetValue(int key, [MaybeNullWhen( false )] out T result)
    {
        if ( key < 0 )
        {
            result = default;
            return false;
        }

        var segmentIndex = key >> _segmentLengthLog2;
        if ( segmentIndex >= _segments.Length )
        {
            result = default;
            return false;
        }

        var itemIndex = key & (SegmentLength - 1);
        ref var segment = ref GetSegmentRef( segmentIndex );
        if ( ! segment.IsInitialized || ! segment.Contains( itemIndex ) )
        {
            result = default;
            return false;
        }

        result = segment.Get( itemIndex );
        return true;
    }

    /// <summary>
    /// Attempts to get a reference to a value associated with the specified <paramref name="key"/>.
    /// Adds a new entry with default value and returns a reference to it, if entry does not exist.
    /// </summary>
    /// <param name="key">Key of a value to get or add.</param>
    /// <param name="exists">An <b>out</b> parameter that returns <b>true</b> when entry exists, otherwise <b>false</b>.</param>
    /// <returns>Reference to existing value or added default value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="key"/> is less than <b>0</b>.</exception>
    public ref T? GetValueRefOrAddDefault(int key, out bool exists)
    {
        Ensure.IsGreaterThanOrEqualTo( key, 0 );
        var segmentIndex = key >> _segmentLengthLog2;
        var itemIndex = key & (SegmentLength - 1);

        if ( segmentIndex < _segments.Length )
        {
            ref var segment = ref GetSegmentRef( segmentIndex );
            if ( segment.IsInitialized )
            {
                if ( segment.Contains( itemIndex ) )
                {
                    exists = true;
                    return ref segment.Get( itemIndex )!;
                }

                var nextCount = checked( Count + 1 );
                segment.Set( itemIndex, default! );
                Count = nextCount;
            }
            else
            {
                var nextCount = checked( Count + 1 );
                segment.Initialize( SegmentLength );
                segment.Set( itemIndex, default! );
                Count = nextCount;
            }

            exists = false;
            return ref segment.Get( itemIndex )!;
        }
        else
        {
            var nextCount = checked( Count + 1 );
            var prevSegments = _segments;
            _segments = new Segment[segmentIndex + 1];
            prevSegments.AsSpan().CopyTo( _segments );
            ref var segment = ref GetSegmentRef( segmentIndex );
            segment.Initialize( SegmentLength );
            segment.Set( itemIndex, default! );
            Count = nextCount;
            exists = false;
            return ref segment.Get( itemIndex )!;
        }
    }

    /// <summary>
    /// Attempts to get a reference to a value associated with the specified <paramref name="key"/>.
    /// Returns a <b>null</b> reference, if entry does not exist.
    /// </summary>
    /// <param name="key">Key of a value to get.</param>
    /// <returns>Reference to existing value or <b>null</b> reference.</returns>
    public ref T GetValueRefOrNullRef(int key)
    {
        if ( key < 0 )
            return ref Unsafe.NullRef<T>();

        var segmentIndex = key >> _segmentLengthLog2;
        if ( segmentIndex >= _segments.Length )
            return ref Unsafe.NullRef<T>();

        var itemIndex = key & (SegmentLength - 1);
        ref var segment = ref GetSegmentRef( segmentIndex );
        if ( ! segment.IsInitialized || ! segment.Contains( itemIndex ) )
            return ref Unsafe.NullRef<T>();

        return ref segment.Get( itemIndex );
    }

    /// <summary>
    /// Adds a new entry or updates existing if <paramref name="key"/> already exists.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="item">Entry's value.</param>
    /// <returns>
    /// <see cref="AddOrUpdateResult.Added"/> when new entry has been added (provided <paramref name="key"/> did not exist),
    /// otherwise <see cref="AddOrUpdateResult.Updated"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="key"/> is less than <b>0</b>.</exception>
    public AddOrUpdateResult AddOrUpdate(int key, T item)
    {
        Ensure.IsGreaterThanOrEqualTo( key, 0 );
        var segmentIndex = key >> _segmentLengthLog2;
        var itemIndex = key & (SegmentLength - 1);

        if ( segmentIndex < _segments.Length )
        {
            ref var segment = ref GetSegmentRef( segmentIndex );
            if ( segment.IsInitialized )
            {
                if ( segment.Contains( itemIndex ) )
                {
                    segment.Set( itemIndex, item );
                    return AddOrUpdateResult.Updated;
                }

                var nextCount = checked( Count + 1 );
                segment.Set( itemIndex, item );
                Count = nextCount;
            }
            else
            {
                var nextCount = checked( Count + 1 );
                segment.Initialize( SegmentLength );
                segment.Set( itemIndex, item );
                Count = nextCount;
            }
        }
        else
        {
            var nextCount = checked( Count + 1 );
            var prevSegments = _segments;
            _segments = new Segment[segmentIndex + 1];
            prevSegments.AsSpan().CopyTo( _segments );
            ref var segment = ref GetSegmentRef( segmentIndex );
            segment.Initialize( SegmentLength );
            segment.Set( itemIndex, item );
            Count = nextCount;
        }

        return AddOrUpdateResult.Added;
    }

    /// <summary>
    /// Attempts to add a new entry.
    /// </summary>
    /// <param name="key">Entry's key.</param>
    /// <param name="item">Entry's value.</param>
    /// <returns><b>true</b> when entry has been added (provided <paramref name="key"/> did not exist), otherwise <b>false</b>.</returns>
    public bool TryAdd(int key, T item)
    {
        if ( key < 0 )
            return false;

        var segmentIndex = key >> _segmentLengthLog2;
        var itemIndex = key & (SegmentLength - 1);

        if ( segmentIndex < _segments.Length )
        {
            ref var segment = ref GetSegmentRef( segmentIndex );
            if ( segment.IsInitialized )
            {
                if ( segment.Contains( itemIndex ) )
                    return false;

                var nextCount = checked( Count + 1 );
                segment.Set( itemIndex, item );
                Count = nextCount;
            }
            else
            {
                var nextCount = checked( Count + 1 );
                segment.Initialize( SegmentLength );
                segment.Set( itemIndex, item );
                Count = nextCount;
            }
        }
        else
        {
            var nextCount = checked( Count + 1 );
            var prevSegments = _segments;
            _segments = new Segment[segmentIndex + 1];
            prevSegments.AsSpan().CopyTo( _segments );
            ref var segment = ref GetSegmentRef( segmentIndex );
            segment.Initialize( SegmentLength );
            segment.Set( itemIndex, item );
            Count = nextCount;
        }

        return true;
    }

    /// <summary>
    /// Attempts to remove an entry with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key of an entry to remove.</param>
    /// <returns><b>true</b> when entry has been removed, otherwise <b>false</b>.</returns>
    public bool Remove(int key)
    {
        if ( key < 0 )
            return false;

        var segmentIndex = key >> _segmentLengthLog2;
        if ( segmentIndex >= _segments.Length )
            return false;

        var itemIndex = key & (SegmentLength - 1);
        ref var segment = ref GetSegmentRef( segmentIndex );
        if ( ! segment.IsInitialized || ! segment.Contains( itemIndex ) )
            return false;

        segment.Remove( itemIndex );
        --Count;
        return true;
    }

    /// <summary>
    /// Attempts to remove an entry with the specified <paramref name="key"/> and to return a value associated with that key.
    /// </summary>
    /// <param name="key">Key of an entry to remove.</param>
    /// <param name="removed">An <b>out</b> parameter that returns a value associated with the <paramref name="key"/>, if it exists.</param>
    /// <returns><b>true</b> when entry has been removed, otherwise <b>false</b>.</returns>
    public bool Remove(int key, [MaybeNullWhen( false )] out T removed)
    {
        if ( key < 0 )
        {
            removed = default;
            return false;
        }

        var segmentIndex = key >> _segmentLengthLog2;
        if ( segmentIndex >= _segments.Length )
        {
            removed = default;
            return false;
        }

        var itemIndex = key & (SegmentLength - 1);
        ref var segment = ref GetSegmentRef( segmentIndex );
        if ( ! segment.IsInitialized || ! segment.Contains( itemIndex ) )
        {
            removed = default;
            return false;
        }

        removed = segment.Get( itemIndex );
        segment.Remove( itemIndex );
        --Count;
        return true;
    }

    /// <summary>
    /// Removes all elements from this dictionary.
    /// </summary>
    public void Clear()
    {
        _segments = Array.Empty<Segment>();
        Count = 0;
    }

    /// <summary>
    /// Attempts to remove unused segments from this dictionary.
    /// </summary>
    public void TrimExcess()
    {
        if ( Count == 0 )
        {
            _segments = Array.Empty<Segment>();
            return;
        }

        var segmentIndex = 0;
        var minSegmentCount = -1;
        do
        {
            ref var segment = ref GetSegmentRef( segmentIndex );
            if ( ! segment.IsInitialized )
                continue;

            if ( segment.ContainsAny() )
                minSegmentCount = segmentIndex;
            else
                segment = default;
        }
        while ( ++segmentIndex < _segments.Length );

        ++minSegmentCount;
        Assume.IsGreaterThan( minSegmentCount, 0 );
        if ( minSegmentCount < _segments.Length )
        {
            var prevSegments = _segments;
            _segments = new Segment[minSegmentCount];
            prevSegments.AsSpan( 0, minSegmentCount ).CopyTo( _segments );
        }
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this dictionary.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _segments, Count, SegmentLength );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="SegmentedSparseDictionary{T}"/>.
    /// </summary>
    public struct Enumerator
    {
        private readonly Segment[]? _segments;
        private readonly int _segmentLength;
        private int _segmentIndex;
        private int _itemIndex;
        private int _remaining;

        internal Enumerator(Segment[]? segments, int count, int segmentLength)
        {
            _segments = segments;
            _segmentLength = segmentLength;
            _segmentIndex = 0;
            _itemIndex = -1;
            _remaining = count;
        }

        /// <summary>
        /// Gets key-value pair at the current position of this enumerator.
        /// </summary>
        public KeyValuePair<int, T> Current
        {
            get
            {
                Assume.IsNotNull( _segments );
                return KeyValuePair.Create( _segmentIndex * _segmentLength + _itemIndex, _segments[_segmentIndex].Get( _itemIndex ) );
            }
        }

        /// <summary>
        /// Advances this enumerator to the next element.
        /// </summary>
        /// <returns><b>true</b> when next element exists, otherwise <b>false</b>.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            if ( _remaining <= 0 )
                return false;

            Assume.IsNotNull( _segments );
            --_remaining;
            while ( true )
            {
                var segment = _segments[_segmentIndex];
                if ( ! segment.IsInitialized )
                {
                    ++_segmentIndex;
                    _itemIndex = -1;
                    continue;
                }

                while ( ++_itemIndex < _segmentLength )
                {
                    if ( segment.Contains( _itemIndex ) )
                        return true;
                }

                ++_segmentIndex;
                _itemIndex = -1;
            }
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private ref Segment GetSegmentRef(int index)
    {
        Assume.IsInRange( index, 0, _segments.Length - 1 );
        ref var first = ref MemoryMarshal.GetArrayDataReference( _segments );
        return ref Unsafe.Add( ref first, index );
    }

    internal struct Segment
    {
        private T[]? _items;
        private byte[]? _existence;

        internal bool IsInitialized => _items is not null;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Initialize(int length)
        {
            Assume.IsGreaterThan( length, 0 );
            Assume.True( BitOperations.IsPow2( length ) );
            Assume.False( IsInitialized );
            _items = new T[length];
            _existence = new byte[length >> 3];
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool Contains(int index)
        {
            Assume.IsNotNull( _existence );
            return ((_existence[index >> 3] >> (index & 7)) & 1) != 0;
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal ref T Get(int index)
        {
            Assume.IsNotNull( _items );
            return ref Unsafe.Add( ref MemoryMarshal.GetArrayDataReference( _items ), index );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Set(int index, T item)
        {
            Assume.IsNotNull( _items );
            Assume.IsNotNull( _existence );
            _items[index] = item;
            _existence[index >> 3] |= ( byte )(1 << (index & 7));
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void Remove(int index)
        {
            Assume.IsNotNull( _items );
            Assume.IsNotNull( _existence );
            _items[index] = default!;
            _existence[index >> 3] &= ( byte )~(1 << (index & 7));
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal bool ContainsAny()
        {
            Assume.IsNotNull( _existence );
            foreach ( var b in _existence )
            {
                if ( b != 0 )
                    return true;
            }

            return false;
        }
    }
}
