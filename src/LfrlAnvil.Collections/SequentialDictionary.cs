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
using System.Linq;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic collection of (key, value) pairs whose insertion order is preserved.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public class SequentialDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _keyIndexMap;
    private SparseListSlim<KeyValuePair<TKey, TValue>> _order;

    /// <summary>
    /// Creates a new empty <see cref="SequentialDictionary{TKey,TValue}"/> instance
    /// with <see cref="EqualityComparer{T}.Default"/> comparer.
    /// </summary>
    public SequentialDictionary()
        : this( EqualityComparer<TKey>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="SequentialDictionary{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="comparer">Key equality comparer.</param>
    public SequentialDictionary(IEqualityComparer<TKey> comparer)
    {
        _keyIndexMap = new Dictionary<TKey, int>( comparer );
        _order = SparseListSlim<KeyValuePair<TKey, TValue>>.Create();
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
    public TValue this[TKey key]
    {
        get
        {
            var index = _keyIndexMap[key];
            return _order[index].Value;
        }
        set
        {
            ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( _keyIndexMap, key, out var exists );
            if ( exists )
            {
                ref var entry = ref _order[index];
                entry = KeyValuePair.Create( key, value );
            }
            else
                index = _order.Add( KeyValuePair.Create( key, value ) );
        }
    }

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => _keyIndexMap.Count;

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );

    /// <inheritdoc />
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    /// <summary>
    /// Key equality comparer.
    /// </summary>
    public IEqualityComparer<TKey> Comparer => _keyIndexMap.Comparer;

    /// <summary>
    /// First entry in order of insertion.
    /// </summary>
    public KeyValuePair<TKey, TValue>? First => _order.First?.Value;

    /// <summary>
    /// Last entry in order of insertion.
    /// </summary>
    public KeyValuePair<TKey, TValue>? Last => _order.Last?.Value;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys.ToList();
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToList();
    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => (( ICollection<KeyValuePair<TKey, int>> )_keyIndexMap).IsReadOnly;

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        var entry = KeyValuePair.Create( key, value );
        var index = _order.Add( entry );
        try
        {
            _keyIndexMap.Add( key, index );
        }
        catch
        {
            _order.Remove( index );
            throw;
        }
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if ( ! _keyIndexMap.Remove( key, out var index ) )
            return false;

        _order.Remove( index );
        return true;
    }

    /// <summary>
    /// Attempts to remove an entry associated with the specified <paramref name="key"/>.
    /// </summary>
    /// <param name="key">Key to remove.</param>
    /// <param name="removed"><b>out</b> parameter that returns removed value associated with the specified <paramref name="key"/>.</param>
    /// <returns><b>true</b> when entry was removed, otherwise <b>false</b>.</returns>
    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        if ( _keyIndexMap.Remove( key, out var index ) )
        {
            ref var entry = ref _order[index];
            removed = entry.Value;
            _order.Remove( index );
            return true;
        }

        removed = default;
        return false;
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey(TKey)" />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _keyIndexMap.ContainsKey( key );
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)" />
    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue result)
    {
        if ( _keyIndexMap.TryGetValue( key, out var index ) )
        {
            ref var entry = ref _order[index];
            result = entry.Value;
            return true;
        }

        result = default;
        return false;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _keyIndexMap.Clear();
        _order.Clear();
    }

    /// <summary>
    /// Creates a new <see cref="SparseListSlimNodeEnumerator{T}"/> instance for this dictionary.
    /// </summary>
    /// <returns>New <see cref="SparseListSlimNodeEnumerator{T}"/> instance.</returns>
    [Pure]
    public SparseListSlimNodeEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new SparseListSlimNodeEnumerator<KeyValuePair<TKey, TValue>>( _order );
    }

    [Pure]
    IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
    {
        return GetEnumerator();
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
        if ( ! _keyIndexMap.TryGetValue( item.Key, out var index ) )
            return false;

        ref var entry = ref _order[index];
        return EqualityComparer<TValue>.Default.Equals( entry.Value, item.Value );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
    {
        Add( item.Key, item.Value );
    }

    bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
    {
        if ( ! _keyIndexMap.TryGetValue( item.Key, out var index ) )
            return false;

        ref var entry = ref _order[index];
        if ( ! EqualityComparer<TValue>.Default.Equals( entry.Value, item.Value ) )
            return false;

        _order.Remove( index );
        return _keyIndexMap.Remove( item.Key );
    }

    void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }
}
