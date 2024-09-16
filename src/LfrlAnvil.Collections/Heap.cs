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

using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Collections;

/// <inheritdoc cref="IHeap{T}" />
public class Heap<T> : IHeap<T>
{
    private ListSlim<T> _items;

    /// <summary>
    /// Creates a new empty <see cref="Heap{T}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    public Heap()
        : this( Comparer<T>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="Heap{T}"/> instance.
    /// </summary>
    /// <param name="comparer">Comparer to use.</param>
    public Heap(IComparer<T> comparer)
    {
        Comparer = comparer;
        _items = ListSlim<T>.Create();
    }

    /// <summary>
    /// Creates a new <see cref="Heap{T}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    /// <param name="collection">Initial collection of entries.</param>
    public Heap(IEnumerable<T> collection)
        : this( collection, Comparer<T>.Default ) { }

    /// <summary>
    /// Creates a new <see cref="Heap{T}"/> instance.
    /// </summary>
    /// <param name="collection">Initial collection of entries.</param>
    /// <param name="comparer">Comparer to use.</param>
    public Heap(IEnumerable<T> collection, IComparer<T> comparer)
    {
        Comparer = comparer;
        _items = ListSlim<T>.Create( collection );

        for ( var i = (_items.Count - 1) >> 1; i >= 0; --i )
            FixDown( i );
    }

    /// <inheritdoc />
    public IComparer<T> Comparer { get; }

    /// <inheritdoc />
    public T this[int index] => _items[index];

    /// <inheritdoc />
    public int Count => _items.Count;

    /// <inheritdoc />
    public void Add(T item)
    {
        _items.Add( item );
        FixUp( _items.Count - 1 );
    }

    /// <inheritdoc />
    public T Extract()
    {
        var result = Peek();
        Pop();
        return result;
    }

    /// <inheritdoc />
    public bool TryExtract([MaybeNullWhen( false )] out T result)
    {
        if ( _items.IsEmpty )
        {
            result = default;
            return false;
        }

        result = Extract();
        return true;
    }

    /// <inheritdoc />
    [Pure]
    public T Peek()
    {
        return _items[0];
    }

    /// <inheritdoc />
    public bool TryPeek([MaybeNullWhen( false )] out T result)
    {
        if ( _items.IsEmpty )
        {
            result = default;
            return false;
        }

        result = Peek();
        return true;
    }

    /// <inheritdoc />
    public void Pop()
    {
        _items[0] = _items[^1];
        _items.RemoveLast();
        FixDown( 0 );
    }

    /// <inheritdoc />
    public bool TryPop()
    {
        if ( _items.IsEmpty )
            return false;

        Pop();
        return true;
    }

    /// <inheritdoc />
    public T Replace(T item)
    {
        var result = Peek();
        _items[0] = item;
        FixDown( 0 );
        return result;
    }

    /// <inheritdoc />
    public bool TryReplace(T item, [MaybeNullWhen( false )] out T replaced)
    {
        if ( _items.IsEmpty )
        {
            replaced = default;
            return false;
        }

        replaced = Replace( item );
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _items.Clear();
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this heap.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _items );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="Heap{T}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<T>
    {
        private readonly ListSlim<T> _items;
        private int _index;

        internal Enumerator(ListSlim<T> items)
        {
            _items = items;
            _index = -1;
        }

        /// <inheritdoc />
        public T Current => _items[_index];

        object? IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return ++_index < _items.Count;
        }

        /// <inheritdoc />
        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixUp(int i)
    {
        ref var first = ref _items.First();

        while ( i > 0 )
        {
            var p = Heap.GetParentIndex( i );
            ref var item = ref Unsafe.Add( ref first, i )!;
            ref var parent = ref Unsafe.Add( ref first, p )!;

            if ( Comparer.Compare( item, parent ) >= 0 )
                break;

            (item, parent) = (parent, item);
            i = p;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixDown(int i)
    {
        ref var first = ref _items.First();
        var l = Heap.GetLeftChildIndex( i );

        while ( l < _items.Count )
        {
            ref var item = ref Unsafe.Add( ref first, i )!;
            ref var target = ref Unsafe.Add( ref first, l )!;

            var t = l;
            if ( Comparer.Compare( item, target ) < 0 )
            {
                t = i;
                target = ref item;
            }

            var r = l + 1;
            if ( r < _items.Count )
            {
                ref var right = ref Unsafe.Add( ref first, r )!;
                if ( Comparer.Compare( right, target ) < 0 )
                {
                    t = r;
                    target = ref right;
                }
            }

            if ( i == t )
                break;

            (item, target) = (target, item);
            i = t;
            l = Heap.GetLeftChildIndex( i );
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
}
