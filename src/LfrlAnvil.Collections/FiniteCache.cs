using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Collections;

public class FiniteCache<TKey, TValue> : IFiniteCache<TKey, TValue>
    where TKey : notnull
{
    private readonly SequentialDictionary<TKey, TValue> _map;

    public FiniteCache(int capacity)
        : this( capacity, EqualityComparer<TKey>.Default ) { }

    public FiniteCache(int capacity, IEqualityComparer<TKey> comparer)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Capacity = capacity;
        _map = new SequentialDictionary<TKey, TValue>( comparer );
    }

    public TValue this[TKey key]
    {
        get => _map[key];
        set
        {
            _map[key] = value;

            if ( Count <= Capacity )
                return;

            Assume.IsNotNull( Oldest );
            Remove( Oldest.Value.Key );
        }
    }

    public int Capacity { get; }
    public int Count => _map.Count;
    public IEnumerable<TKey> Keys => _map.Keys;
    public IEnumerable<TValue> Values => _map.Values;
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public KeyValuePair<TKey, TValue>? Oldest => _map.First;
    public KeyValuePair<TKey, TValue>? Newest => _map.Last;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => (( IDictionary<TKey, TValue> )_map).Keys;
    ICollection<TValue> IDictionary<TKey, TValue>.Values => (( IDictionary<TKey, TValue> )_map).Values;
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => (( ICollection<KeyValuePair<TKey, TValue>> )_map).IsReadOnly;

    public void Add(TKey key, TValue value)
    {
        _map.Add( key, value );

        if ( Count <= Capacity )
            return;

        Assume.IsNotNull( Oldest );
        _map.Remove( Oldest.Value.Key );
    }

    public bool Remove(TKey key)
    {
        return _map.Remove( key );
    }

    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        return _map.Remove( key, out removed );
    }

    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        return _map.TryGetValue( key, out value );
    }

    public void Clear()
    {
        _map.Clear();
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _map.GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    bool IDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        return (( IDictionary<TKey, TValue> )_map).TryGetValue( key, out value );
    }

    bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        return (( IReadOnlyDictionary<TKey, TValue> )_map).TryGetValue( key, out value );
    }

    [Pure]
    bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
    {
        return (( ICollection<KeyValuePair<TKey, TValue>> )_map).Contains( item );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        (( ICollection<KeyValuePair<TKey, TValue>> )_map).Add( item );
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        return (( ICollection<KeyValuePair<TKey, TValue>> )_map).Remove( item );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        (( ICollection<KeyValuePair<TKey, TValue>> )_map).CopyTo( array, arrayIndex );
    }
}
