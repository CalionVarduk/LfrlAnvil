using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlSoft.NET.Core.Extensions;

namespace LfrlSoft.NET.Core.Collections
{
    public class Heap<T> : IHeap<T>
    {
        private readonly List<T> _items;

        public Heap()
            : this( Comparer<T>.Default ) { }

        public Heap(IComparer<T> comparer)
        {
            Comparer = comparer;
            _items = new List<T>();
        }

        public Heap(IEnumerable<T> collection)
            : this( collection, Comparer<T>.Default ) { }

        public Heap(IEnumerable<T> collection, IComparer<T> comparer)
        {
            Comparer = comparer;
            _items = collection.ToList();

            for ( var i = (_items.Count - 1) >> 1; i >= 0; --i )
                FixDown( i );
        }

        public IComparer<T> Comparer { get; }
        public T this[int index] => _items[index];
        public int Count => _items.Count;

        public void Add(T item)
        {
            _items.Add( item );
            FixUp( _items.Count - 1 );
        }

        public T Extract()
        {
            var result = Peek();
            Pop();
            return result;
        }

        public bool TryExtract([MaybeNullWhen( false )] out T result)
        {
            if ( _items.Count == 0 )
            {
                result = default;
                return false;
            }

            result = Extract();
            return true;
        }

        [Pure]
        public T Peek()
        {
            return _items[0];
        }

        public bool TryPeek([MaybeNullWhen( false )] out T result)
        {
            if ( _items.Count == 0 )
            {
                result = default;
                return false;
            }

            result = Peek();
            return true;
        }

        public void Pop()
        {
            _items[0] = _items[^1];
            _items.RemoveAt( _items.Count - 1 );
            FixDown( 0 );
        }

        public bool TryPop()
        {
            if ( _items.Count == 0 )
                return false;

            Pop();
            return true;
        }

        public T Replace(T item)
        {
            var result = Peek();
            _items[0] = item;
            FixDown( 0 );
            return result;
        }

        public bool TryReplace(T item, [MaybeNullWhen( false )] out T replaced)
        {
            if ( _items.Count == 0 )
            {
                replaced = default;
                return false;
            }

            replaced = Replace( item );
            return true;
        }

        public void Clear()
        {
            _items.Clear();
        }

        [Pure]
        public IEnumerator<T> GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void FixUp(int i)
        {
            var p = Heap.GetParentIndex( i );

            while ( i > 0 && Comparer.Compare( _items[i], _items[p] ) < 0 )
            {
                _items.SwapItems( i, p );
                i = p;
                p = Heap.GetParentIndex( i );
            }
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void FixDown(int i)
        {
            var l = Heap.GetLeftChildIndex( i );

            while ( l < _items.Count )
            {
                var r = l + 1;
                var m = Comparer.Compare( _items[l], _items[i] ) < 0 ? l : i;

                if ( r < _items.Count && Comparer.Compare( _items[r], _items[m] ) < 0 )
                    m = r;

                if ( m == i )
                    break;

                _items.SwapItems( i, m );
                i = m;
                l = Heap.GetLeftChildIndex( i );
            }
        }
    }
}
