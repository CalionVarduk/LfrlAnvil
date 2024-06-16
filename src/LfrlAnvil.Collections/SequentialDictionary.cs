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
using LfrlAnvil.Internal;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic collection of (key, value) pairs whose insertion order is preserved.
/// </summary>
/// <typeparam name="TKey">Key type.</typeparam>
/// <typeparam name="TValue">Value type.</typeparam>
public class SequentialDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>> _map;
    private DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>> _order;

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
        _map = new Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>>( comparer );
        _order = DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>>.Empty;
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.this" />
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

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => _map.Count;

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );

    /// <inheritdoc />
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    /// <summary>
    /// Key equality comparer.
    /// </summary>
    public IEqualityComparer<TKey> Comparer => _map.Comparer;

    /// <summary>
    /// First entry in order of insertion.
    /// </summary>
    public KeyValuePair<TKey, TValue>? First => _order.Head?.Value;

    /// <summary>
    /// Last entry in order of insertion.
    /// </summary>
    public KeyValuePair<TKey, TValue>? Last => _order.Tail?.Value;

    ICollection<TKey> IDictionary<TKey, TValue>.Keys => Keys.ToList();
    ICollection<TValue> IDictionary<TKey, TValue>.Values => Values.ToList();

    bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly =>
        (( ICollection<KeyValuePair<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>>> )_map).IsReadOnly;

    /// <inheritdoc />
    public void Add(TKey key, TValue value)
    {
        var node = new DoublyLinkedNode<KeyValuePair<TKey, TValue>>( KeyValuePair.Create( key, value ) );
        _map.Add( key, node );
        _order = _order.AddLast( node );
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if ( ! _map.Remove( key, out var node ) )
            return false;

        _order = _order.Remove( node );
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
        if ( _map.Remove( key, out var node ) )
        {
            removed = node.Value.Value;
            _order = _order.Remove( node );
            return true;
        }

        removed = default;
        return false;
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.ContainsKey(TKey)" />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    /// <inheritdoc cref="IDictionary{TKey,TValue}.TryGetValue(TKey,out TValue)" />
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

    /// <inheritdoc />
    public void Clear()
    {
        _map.Clear();
        _order = _order.Clear();
    }

    /// <inheritdoc />
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
