using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Collections;

public class SequentialDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>> _map;
    private DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>> _order;

    public SequentialDictionary()
        : this( EqualityComparer<TKey>.Default ) { }

    public SequentialDictionary(IEqualityComparer<TKey> comparer)
    {
        _map = new Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>>( comparer );
        _order = DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>>.Empty;
    }

    public TValue this[TKey key]
    {
        get => _map[key].Value.Value;
        set
        {
            ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
            if ( exists )
                node.Value = KeyValuePair.Create( key, value );
            else
            {
                node = new DoublyLinkedNode<KeyValuePair<TKey, TValue>>( KeyValuePair.Create( key, value ) );
                _order = _order.AddLast( node );
            }
        }
    }

    public int Count => _map.Count;
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public KeyValuePair<TKey, TValue>? First => _order.Head?.Value;
    public KeyValuePair<TKey, TValue>? Last => _order.Tail?.Value;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys.ToList();
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToList();

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
        (( ICollection<KeyValuePair<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>>> )_map).IsReadOnly;

    public void Add(TKey key, TValue value)
    {
        var node = new DoublyLinkedNode<KeyValuePair<TKey, TValue>>( KeyValuePair.Create( key, value ) );
        _map.Add( key, node );
        _order = _order.AddLast( node );
    }

    public bool Remove(TKey key)
    {
        if ( ! _map.Remove( key, out var node ) )
            return false;

        _order = _order.Remove( node );
        return true;
    }

    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        if ( _map.Remove( key, out var node ) )
        {
            removed = node.Value.Value;
            _order = _order.Remove( node );
            return true;
        }

        removed = default;
        return false;
    }

    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result)
    {
        if ( _map.TryGetValue( key, out var node ) )
        {
            result = node.Value.Value;
            return true;
        }

        result = default;
        return false;
    }

    public void Clear()
    {
        _map.Clear();
        _order = _order.Clear();
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _order.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue result)
    {
        return TryGetValue( key, out result! );
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue result)
    {
        return TryGetValue( key, out result! );
    }

    [Pure]
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return _map.TryGetValue( item.Key, out var node ) && EqualityComparer<TValue>.Default.Equals( node.Value.Value, item.Value );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add( item.Key, item.Value );
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if ( ! _map.TryGetValue( item.Key, out var node ) || ! EqualityComparer<TValue>.Default.Equals( node.Value.Value, item.Value ) )
            return false;

        _order = _order.Remove( node );
        return _map.Remove( item.Key );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }
}
