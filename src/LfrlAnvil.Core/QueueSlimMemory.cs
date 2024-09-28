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
using System.Runtime.InteropServices;

namespace LfrlAnvil;

/// <summary>
/// Represents a view of a <see cref="QueueSlim{T}"/> instance's circular buffer.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public readonly struct QueueSlimMemory<T>
{
    /// <summary>
    /// Empty <see cref="QueueSlim{T}"/> view.
    /// </summary>
    public static QueueSlimMemory<T> Empty => new QueueSlimMemory<T>( ReadOnlyMemory<T>.Empty, ReadOnlyMemory<T>.Empty );

    internal QueueSlimMemory(ReadOnlyMemory<T> first, ReadOnlyMemory<T> second)
    {
        First = first;
        Second = second;
    }

    /// <summary>
    /// First range of elements in queue's circular buffer.
    /// </summary>
    public ReadOnlyMemory<T> First { get; }

    /// <summary>
    /// Second range of elements in queue's circular buffer.
    /// </summary>
    public ReadOnlyMemory<T> Second { get; }

    /// <summary>
    /// Gets the number of elements in this view.
    /// </summary>
    public int Length => First.Length + Second.Length;

    /// <summary>
    /// Gets a reference to an element at the given index.
    /// </summary>
    /// <param name="index">The zero-based index of the element reference to get.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="index"/> is out of bounds.</exception>
    public ref T this[int index]
    {
        get
        {
            Ensure.IsInIndexRange( index, Length );
            if ( index < First.Length )
            {
                ref var first = ref MemoryMarshal.GetReference( First.Span );
                return ref Unsafe.Add( ref first, index );
            }
            else
            {
                ref var first = ref MemoryMarshal.GetReference( Second.Span );
                return ref Unsafe.Add( ref first, index - First.Length );
            }
        }
    }

    /// <summary>
    /// Creates a new <see cref="QueueSlimMemory{T}"/> instance from the provided region of memory.
    /// </summary>
    /// <param name="items">Source range of elements.</param>
    /// <returns>New <see cref="QueueSlimMemory{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static QueueSlimMemory<T> From(ReadOnlyMemory<T> items)
    {
        return new QueueSlimMemory<T>( items, ReadOnlyMemory<T>.Empty );
    }

    /// <summary>
    /// Forms a slice out of this view, beginning at a specified position and continuing to its end.
    /// </summary>
    /// <param name="startIndex">The index at which to begin this slice.</param>
    /// <returns>New <see cref="QueueSlimMemory{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="startIndex"/> is out of bounds.</exception>
    [Pure]
    public QueueSlimMemory<T> Slice(int startIndex)
    {
        return Slice( startIndex, Length - startIndex );
    }

    /// <summary>
    /// Forms a slice out of this view, starting at <paramref name="startIndex"/> position for <paramref name="length"/> elements.
    /// </summary>
    /// <param name="startIndex">The index at which to begin this slice.</param>
    /// <param name="length">The desired length for the slice.</param>
    /// <returns>New <see cref="QueueSlimMemory{T}"/> instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="startIndex"/> or <paramref name="length"/> are out of bounds.
    /// </exception>
    [Pure]
    public QueueSlimMemory<T> Slice(int startIndex, int length)
    {
        if ( startIndex < First.Length )
        {
            var firstLength = First.Length - startIndex;
            return firstLength >= length
                ? new QueueSlimMemory<T>( First.Slice( startIndex, length ), ReadOnlyMemory<T>.Empty )
                : new QueueSlimMemory<T>( First.Slice( startIndex ), Second.Slice( 0, length - firstLength ) );
        }

        startIndex -= First.Length;
        return new QueueSlimMemory<T>( Second.Slice( startIndex, length ), ReadOnlyMemory<T>.Empty );
    }

    /// <summary>
    /// Copies the contents of this view into the <paramref name="buffer"/>.
    /// </summary>
    /// <param name="buffer">The span to copy elements into.</param>
    /// <exception cref="ArgumentException">When <paramref name="buffer"/> is too short.</exception>
    public void CopyTo(Span<T> buffer)
    {
        First.Span.CopyTo( buffer );
        Second.Span.CopyTo( buffer.Slice( First.Length ) );
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this view.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( First.Span, Second.Span );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="QueueSlimMemory{T}"/>.
    /// </summary>
    public ref struct Enumerator
    {
        private readonly ReadOnlySpan<T> _first;
        private readonly ReadOnlySpan<T> _second;
        private readonly int _maxIndex;
        private int _index;

        internal Enumerator(ReadOnlySpan<T> first, ReadOnlySpan<T> second)
        {
            _first = first;
            _second = second;
            _maxIndex = _first.Length + _second.Length - 1;
            _index = -1;
        }

        /// <summary>
        /// Gets a reference to the element in the view at the current position of the enumerator.
        /// </summary>
        public ref T Current
        {
            get
            {
                if ( _index < _first.Length )
                {
                    ref var first = ref MemoryMarshal.GetReference( _first );
                    return ref Unsafe.Add( ref first, _index );
                }
                else
                {
                    ref var first = ref MemoryMarshal.GetReference( _second );
                    return ref Unsafe.Add( ref first, _index - _first.Length );
                }
            }
        }

        /// <summary>
        /// Advances the enumerator to the next element of the collection.
        /// </summary>
        /// <returns><b>true</b> if the enumerator was successfully advanced to the next element, otherwise <b>false</b>.</returns>
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            if ( _index >= _maxIndex )
                return false;

            ++_index;
            return true;
        }
    }
}
