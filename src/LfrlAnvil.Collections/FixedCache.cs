using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Collections
{
    // TODO: this is an early draft, FixedCache should also implement IDictionary<TKey, TValue>
    // also, an OrderedDictionary<TKey, TValue> collection could serve as an underlying data structure
    // OrderedDictionary would remember the order of element insertion, which would allow for an easy removal of any FixedCache elements
    // without breaking the underlying order
    // OrderedDictionary will use an underlying LinkedList<(TKey Key, TValue Value)> structure
    // as well as Dictionary<TKey, LinkedListNode<(TKey Key, TValue Value)>>
    public class FixedCache<TKey, TValue> : IFixedCache<TKey, TValue>
        where TKey : notnull
    {
        private sealed class Node
        {
            internal Node(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }

            internal TKey Key { get; }
            internal TValue Value { get; private set; }

            internal void SetValue(TValue value)
            {
                Value = value;
            }
        }

        private readonly Dictionary<TKey, Node> _map;
        private readonly Ring<Node> _order;

        public FixedCache(int capacity)
            : this( capacity, EqualityComparer<TKey>.Default ) { }

        public FixedCache(int capacity, IEqualityComparer<TKey> comparer)
        {
            _order = new Ring<Node>( capacity );
            _map = new Dictionary<TKey, Node>( comparer );
        }

        public TValue this[TKey key]
        {
            get => _map[key].Value;
            set
            {
                if ( _map.TryGetValue( key, out var node ) )
                {
                    node.SetValue( value );
                    return;
                }

                Add( key, value );
            }
        }

        public int Capacity => _order.Count;
        public int Count => _map.Count;
        public IEqualityComparer<TKey> Comparer => _map.Comparer;
        public IEnumerable<TKey> Keys => GetActiveNodesInOrder().Select( n => n.Key );
        public IEnumerable<TValue> Values => GetActiveNodesInOrder().Select( n => n.Value );

        public KeyValuePair<TKey, TValue>? Newest
        {
            get
            {
                if ( Count == 0 )
                    return null;

                var node = _order[_order.WriteIndex == 0 ? Capacity - 1 : _order.WriteIndex - 1]!;
                return KeyValuePair.Create( node.Key, node.Value );
            }
        }

        public KeyValuePair<TKey, TValue>? Oldest
        {
            get
            {
                if ( Count == 0 )
                    return null;

                var node = _order[Count == Capacity ? _order.WriteIndex : 0]!;
                return KeyValuePair.Create( node.Key, node.Value );
            }
        }

        [Pure]
        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey( key );
        }

        public bool TryGetValue(TKey key, out TValue value)
        {
            if ( _map.TryGetValue( key, out var node ) )
            {
                value = node.Value;
                return true;
            }

            value = default!;
            return false;
        }

        public bool TryAdd(TKey key, TValue value)
        {
            var node = new Node( key, value );

            if ( ! _map.TryAdd( key, node ) )
                return false;

            FixOrder( node );
            return true;
        }

        public void Add(TKey key, TValue value)
        {
            var node = new Node( key, value );
            _map.Add( key, node );
            FixOrder( node );
        }

        public void Clear()
        {
            _order.Clear();
            _map.Clear();
        }

        [Pure]
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            var activeNodes = GetActiveNodesInOrder();
            return activeNodes.Select( n => KeyValuePair.Create( n.Key, n.Value ) ).GetEnumerator();
        }

        private void FixOrder(Node newNode)
        {
            if ( Count > Capacity )
                _map.Remove( _order[_order.WriteIndex]!.Key );

            _order.SetNext( newNode );
        }

        private IEnumerable<Node> GetActiveNodesInOrder()
        {
            return _order.Read( _order.WriteIndex - Count ).Take( Count )!;
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
