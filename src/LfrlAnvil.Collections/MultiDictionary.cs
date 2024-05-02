using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections;

/// <inheritdoc />
public class MultiDictionary<TKey, TValue> : IMultiDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, ValuesCollection> _map;

    /// <summary>
    /// Creates a new empty <see cref="MultiDictionary{TKey,TValue}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> key comparer.
    /// </summary>
    public MultiDictionary()
        : this( EqualityComparer<TKey>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="MultiDictionary{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="comparer">Key equality comparer.</param>
    public MultiDictionary(IEqualityComparer<TKey> comparer)
    {
        _map = new Dictionary<TKey, ValuesCollection>( comparer );
    }

    /// <inheritdoc cref="IMultiDictionary{TKey,TValue}.this" />
    public IReadOnlyList<TValue> this[TKey key]
    {
        get => _map.TryGetValue( key, out var result ) ? result : Array.Empty<TValue>();
        set => SetRange( key, value );
    }

    /// <inheritdoc cref="IMultiDictionary{TKey,TValue}.Count" />
    public int Count => _map.Count;

    /// <inheritdoc />
    public IEqualityComparer<TKey> Comparer => _map.Comparer;

    /// <inheritdoc cref="IMultiDictionary{TKey,TValue}.Keys" />
    public IReadOnlyCollection<TKey> Keys => _map.Keys;

    /// <inheritdoc cref="IMultiDictionary{TKey,TValue}.Values" />
    public IReadOnlyCollection<IReadOnlyList<TValue>> Values => _map.Values;

    IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => this[key];

    ICollection<TKey> IDictionary<TKey, IReadOnlyList<TValue>>.Keys => _map.Keys;
    ICollection<IReadOnlyList<TValue>> IDictionary<TKey, IReadOnlyList<TValue>>.Values => Values.ToList();

    IEnumerable<TKey> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.Keys => _map.Keys;
    IEnumerable<IReadOnlyList<TValue>> IReadOnlyDictionary<TKey, IReadOnlyList<TValue>>.Values => _map.Values;

    bool ICollection<KeyValuePair<TKey, IReadOnlyList<TValue>>>.IsReadOnly =>
        (( ICollection<KeyValuePair<TKey, ValuesCollection>> )_map).IsReadOnly;

    /// <inheritdoc cref="IMultiDictionary{TKey,TValue}.ContainsKey(TKey)" />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    /// <inheritdoc />
    [Pure]
    public int GetCount(TKey key)
    {
        return _map.TryGetValue( key, out var list ) ? list.Count : 0;
    }

    /// <inheritdoc cref="IMultiDictionary{TKey,TValue}.TryGetValue(TKey,out IReadOnlyList{TValue})" />
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

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( ! exists )
            list = new ValuesCollection( key );

        list.Add( value );
    }

    /// <inheritdoc />
    public void AddRange(TKey key, IEnumerable<TValue> values)
    {
        using var enumerator = values.GetEnumerator();
        if ( ! enumerator.MoveNext() )
            return;

        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( ! exists )
            list = new ValuesCollection( key );

        list.Add( enumerator.Current );
        while ( enumerator.MoveNext() )
            list.Add( enumerator.Current );
    }

    /// <inheritdoc />
    public void SetRange(TKey key, IEnumerable<TValue> values)
    {
        using var enumerator = values.GetEnumerator();
        if ( ! enumerator.MoveNext() )
        {
            _map.Remove( key );
            return;
        }

        ref var list = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( exists )
            list.Clear();
        else
            list = new ValuesCollection( key );

        list.Add( enumerator.Current );
        while ( enumerator.MoveNext() )
            list.Add( enumerator.Current );
    }

    /// <inheritdoc />
    public IReadOnlyList<TValue> Remove(TKey key)
    {
        return _map.Remove( key, out var list ) ? list : Array.Empty<TValue>();
    }

    /// <inheritdoc />
    public bool Remove(TKey key, TValue value)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return false;

        var removed = list.Remove( value );
        if ( removed && list.Count == 0 )
            _map.Remove( key );

        return removed;
    }

    /// <inheritdoc />
    public bool RemoveAt(TKey key, int index)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return false;

        list.RemoveAt( index );
        if ( list.Count == 0 )
            _map.Remove( key );

        return true;
    }

    /// <inheritdoc />
    public bool RemoveRange(TKey key, int index, int count)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return false;

        list.RemoveRange( index, count );
        if ( list.Count == 0 )
            _map.Remove( key );

        return true;
    }

    /// <inheritdoc />
    public int RemoveAll(TKey key, Predicate<TValue> predicate)
    {
        if ( ! _map.TryGetValue( key, out var list ) )
            return 0;

        var removed = list.RemoveAll( predicate );
        if ( removed > 0 && list.Count == 0 )
            _map.Remove( key );

        return removed;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _map.Clear();
    }

    /// <inheritdoc cref="IEnumerable{T}.GetEnumerator()" />
    [Pure]
    public IEnumerator<KeyValuePair<TKey, IReadOnlyList<TValue>>> GetEnumerator()
    {
        return _map.Select( static kv => KeyValuePair.Create( kv.Key, ( IReadOnlyList<TValue> )kv.Value ) ).GetEnumerator();
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
        return _map.Select( static kv => ( IGrouping<TKey, TValue> )kv.Value ).GetEnumerator();
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
