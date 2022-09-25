using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections;

public class MultiDictionary<TKey, TValue> : IMultiDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, ValuesCollection> _map;

    public MultiDictionary()
        : this( EqualityComparer<TKey>.Default ) { }

    public MultiDictionary(IEqualityComparer<TKey> comparer)
    {
        _map = new Dictionary<TKey, ValuesCollection>( comparer );
    }

    public IReadOnlyList<TValue> this[TKey key]
    {
        get => _map.TryGetValue( key, out var result ) ? result : Array.Empty<TValue>();
        set => SetRange( key, value );
    }

    public int Count => _map.Count;
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public IReadOnlyCollection<TKey> Keys => _map.Keys;
    public IReadOnlyCollection<IReadOnlyList<TValue>> Values => _map.Values;

    IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => this[key];

    ICollection<TKey> IDictionary<TKey, IReadOnlyList<TValue>>.Keys => _map.Keys;
    ICollection<IReadOnlyList<TValue>> IDictionary<TKey, IReadOnlyList<TValue>>.Values => Values.ToList();

    IEnumerable<TKey> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.Keys => _map.Keys;
    IEnumerable<IReadOnlyList<TValue>> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.Values => _map.Values;

    bool ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>>.IsReadOnly =>
        ((ICollection<KeyValuePair<TKey, ValuesCollection>>)_map).IsReadOnly;

    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    [Pure]
    public int GetCount(TKey key)
    {
        return _map.TryGetValue( key, out var list ) ? list.Count : 0;
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out IReadOnlyList<TValue> value)
    {
        if ( _map.TryGetValue( key, out var list ) )
        {
            value = list;
            return true;
        }

        value = default;
        return false;
    }

    public void Add(TKey key, TValue value)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
        {
            list = new ValuesCollection( key );
            _map.Add( key, list );
        }

        list.Add( value );
    }

    public void AddRange(TKey key, IEnumerable<TValue> values)
    {
        using var enumerator = values.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return;

        if ( ! _map.TryGetValue( key, out var list ) )
        {
            list = new ValuesCollection( key );
            _map.Add( key, list );
        }

        list.Add( enumerator.Current );
        while ( enumerator.MoveNext() )
            list.Add( enumerator.Current );
    }

    public void SetRange(TKey key, IEnumerable<TValue> values)
    {
        using var enumerator = values.GetEnumerator();
        if ( ! enumerator.MoveNext() )
        {
            _map.Remove( key );
            return;
        }

        if ( _map.TryGetValue( key, out var list ) )
            list.Clear();
        else
        {
            list = new ValuesCollection( key );
            _map.Add( key, list );
        }

        list.Add( enumerator.Current );
        while ( enumerator.MoveNext() )
            list.Add( enumerator.Current );
    }

    public IReadOnlyList<TValue> Remove(TKey key)
    {
        return _map.Remove( key, out var list ) ? list : Array.Empty<TValue>();
    }

    public bool Remove(TKey key, TValue value)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return false;

        var removed = list.Remove( value );
        if ( removed && list.Count == 0 )
            _map.Remove( key );

        return removed;
    }

    public bool RemoveAt(TKey key, int index)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return false;

        list.RemoveAt( index );
        if ( list.Count == 0 )
            _map.Remove( key );

        return true;
    }

    public bool RemoveRange(TKey key, int index, int count)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return false;

        list.RemoveRange( index, count );
        if ( list.Count == 0 )
            _map.Remove( key );

        return true;
    }

    public int RemoveAll(TKey key, Predicate<TValue> predicate)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return 0;

        var removed = list.RemoveAll( predicate );
        if ( removed > 0 && list.Count == 0 )
            _map.Remove( key );

        return removed;
    }

    public void Clear()
    {
        _map.Clear();
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetEnumerator()
    {
        return _map.Select( kv => KeyValuePair.Create( kv.Key, (IReadOnlyList<TValue>)kv.Value ) ).GetEnumerator();
    }

    [Pure]
    bool ILookup<TKey, TValue>.Contains(TKey key)
    {
        return _map.ContainsKey( key );
    }

    void IDictionary<TKey, IReadOnlyList<TValue>>.Add(TKey key, IReadOnlyList<TValue> values)
    {
        AddRange( key, values );
    }

    bool IDictionary<TKey, IReadOnlyList<TValue>>.Remove(TKey key)
    {
        return Remove( key ).Count > 0;
    }

    void ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>>.Add(KeyValuePair<TKey, IReadOnlyList<TValue>> item)
    {
        AddRange( item.Key, item.Value );
    }

    bool ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>>.Remove(KeyValuePair<TKey, IReadOnlyList<TValue>> item)
    {
        return RemoveAll( item.Key, v => item.Value.Contains( v ) ) > 0;
    }

    bool ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>>.Contains(KeyValuePair<TKey, IReadOnlyList<TValue>> item)
    {
        var values = this[item.Key];
        return item.Value.All( v => values.Contains( v ) );
    }

    void ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>>.CopyTo(KeyValuePair<TKey, IReadOnlyList<TValue>>[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }

    [Pure]
    IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator()
    {
        return _map.Select( kv => (IGrouping<TKey, TValue>)kv.Value ).GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private sealed class ValuesCollection : List<TValue>, IGrouping<TKey, TValue>
    {
        internal ValuesCollection(TKey key)
        {
            Key = key;
        }

        public TKey Key { get; }
    }
}
