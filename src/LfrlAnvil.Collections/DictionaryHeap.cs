using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections
{
    public class DictionaryHeap<TKey, TValue> : IDictionaryHeap<TKey, TValue>
    {
        private readonly List<DictionaryHeapNode<TKey, TValue>> _items;
        private readonly Dictionary<TKey, DictionaryHeapNode<TKey, TValue>> _map;

        public DictionaryHeap()
            : this( EqualityComparer<TKey>.Default, Comparer<TValue>.Default ) { }

        public DictionaryHeap(IEqualityComparer<TKey> keyComparer, IComparer<TValue> comparer)
        {
            Comparer = comparer;
            _items = new List<DictionaryHeapNode<TKey, TValue>>();
            _map = new Dictionary<TKey, DictionaryHeapNode<TKey, TValue>>( keyComparer );
        }

        public DictionaryHeap(IEnumerable<KeyValuePair<TKey, TValue>> collection)
            : this( collection, EqualityComparer<TKey>.Default, Comparer<TValue>.Default ) { }

        public DictionaryHeap(
            IEnumerable<KeyValuePair<TKey, TValue>> collection,
            IEqualityComparer<TKey> keyComparer,
            IComparer<TValue> comparer)
        {
            Comparer = comparer;
            _items = new List<DictionaryHeapNode<TKey, TValue>>();
            _map = new Dictionary<TKey, DictionaryHeapNode<TKey, TValue>>( keyComparer );

            foreach ( var (key, value) in collection )
            {
                var node = CreateNode( key, value );
                _map.Add( key, node );
                _items.Add( node );
            }

            for ( var i = (_items.Count - 1) >> 1; i >= 0; --i )
                FixDown( i );
        }

        public IComparer<TValue> Comparer { get; }
        public IEqualityComparer<TKey> KeyComparer => _map.Comparer;
        public TValue this[int index] => _items[index].Value;
        public int Count => _items.Count;

        [Pure]
        public TKey GetKey(int index)
        {
            return _items[index].Key;
        }

        [Pure]
        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey( key );
        }

        [Pure]
        public TValue GetValue(TKey key)
        {
            return _map[key].Value;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result)
        {
            if ( _map.TryGetValue( key, out var node ) )
            {
                result = node.Value;
                return true;
            }

            result = default;
            return false;
        }

        [Pure]
        public TValue Peek()
        {
            return _items[0].Value;
        }

        public bool TryPeek([MaybeNullWhen( false )] out TValue result)
        {
            if ( _items.Count == 0 )
            {
                result = default;
                return false;
            }

            result = Peek();
            return true;
        }

        public TValue Extract()
        {
            var result = Peek();
            Pop();
            return result;
        }

        public bool TryExtract([MaybeNullWhen( false )] out TValue result)
        {
            if ( _items.Count == 0 )
            {
                result = default;
                return false;
            }

            result = Extract();
            return true;
        }

        public void Add(TKey key, TValue value)
        {
            var node = CreateNode( key, value );
            _map.Add( key, node );
            _items.Add( node );
            FixUp( _items.Count - 1 );
        }

        public bool TryAdd(TKey key, TValue value)
        {
            var node = CreateNode( key, value );
            if ( ! _map.TryAdd( key, node ) )
                return false;

            _items.Add( node );
            FixUp( _items.Count - 1 );
            return true;
        }

        public TValue Remove(TKey key)
        {
            if ( ! TryRemove( key, out var removed ) )
                throw new KeyNotFoundException( $"The given key '{key}' was not present in the dictionary." );

            return removed;
        }

        public bool TryRemove(TKey key, [MaybeNullWhen( false )] out TValue removed)
        {
            if ( _map.Remove( key, out var node ) )
            {
                var lastNode = _items[^1];
                _items[node.Index] = lastNode;
                lastNode.AssignIndexFrom( node );
                _items.RemoveLast();

                if ( node.Index < _items.Count )
                    FixRelative( lastNode, node.Value );

                removed = node.Value;
                return true;
            }

            removed = default;
            return false;
        }

        public void Pop()
        {
            var nodeToPop = _items[0];
            var lastNode = _items[^1];
            _items[0] = lastNode;
            lastNode.AssignIndexFrom( nodeToPop );
            _items.RemoveLast();
            _map.Remove( nodeToPop.Key );
            FixDown( 0 );
        }

        public bool TryPop()
        {
            if ( _items.Count == 0 )
                return false;

            Pop();
            return true;
        }

        public TValue Replace(TKey key, TValue value)
        {
            var node = _map[key];
            return Replace( node, value );
        }

        public bool TryReplace(TKey key, TValue value, [MaybeNullWhen( false )] out TValue replaced)
        {
            if ( _map.TryGetValue( key, out var node ) )
            {
                replaced = Replace( node, value );
                return true;
            }

            replaced = default;
            return false;
        }

        public TValue AddOrReplace(TKey key, TValue value)
        {
            if ( _map.TryGetValue( key, out var node ) )
                return Replace( node, value );

            Add( key, value );
            return value;
        }

        public void Clear()
        {
            _items.Clear();
            _map.Clear();
        }

        [Pure]
        public IEnumerator<TValue> GetEnumerator()
        {
            return _items.Select( n => n.Value ).GetEnumerator();
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private DictionaryHeapNode<TKey, TValue> CreateNode(TKey key, TValue value)
        {
            return new DictionaryHeapNode<TKey, TValue>( key, value, _items.Count );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private TValue Replace(DictionaryHeapNode<TKey, TValue> node, TValue value)
        {
            var oldValue = node.Value;
            node.Value = value;
            FixRelative( node, oldValue );
            return oldValue;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void FixRelative(DictionaryHeapNode<TKey, TValue> node, TValue oldValue)
        {
            if ( Comparer.Compare( oldValue, node.Value ) < 0 )
                FixDown( node.Index );
            else
                FixUp( node.Index );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private void FixUp(int i)
        {
            var p = Heap.GetParentIndex( i );

            while ( i > 0 )
            {
                var node = _items[i];
                var parentNode = _items[p];

                if ( Comparer.Compare( node.Value, parentNode.Value ) >= 0 )
                    break;

                _items.SwapItems( i, p );
                node.SwapIndexWith( parentNode );
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
                var node = _items[i];
                var leftChildNode = _items[l];

                var r = l + 1;
                var nodeToSwap = Comparer.Compare( leftChildNode.Value, node.Value ) < 0 ? leftChildNode : node;

                if ( r < _items.Count )
                {
                    var rightChildNode = _items[r];
                    if ( Comparer.Compare( rightChildNode.Value, nodeToSwap.Value ) < 0 )
                        nodeToSwap = rightChildNode;
                }

                if ( ReferenceEquals( node, nodeToSwap ) )
                    break;

                _items.SwapItems( i, nodeToSwap.Index );
                node.SwapIndexWith( nodeToSwap );
                i = node.Index;
                l = Heap.GetLeftChildIndex( i );
            }
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
