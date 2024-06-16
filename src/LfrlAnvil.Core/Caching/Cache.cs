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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Caching;

/// <inheritdoc cref="ICache{TKey,TValue}" />
/// <remarks>New entries added to this cache are added as <see cref="Newest"/>.</remarks>
public sealed class Cache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>> _map;
    private DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>> _order;

    /// <summary>
    /// Creates a new empty <see cref="Cache{TKey,TValue}"/> instance that uses the <see cref="EqualityComparer{T}.Default"/> key comparer.
    /// </summary>
    /// <param name="capacity">An optional maximum capacity. Equal to <see cref="Int32.MaxValue"/> by default.</param>
    /// <param name="removeCallback">An optional callback which gets invoked every time an entry is removed from this cache.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="capacity"/> is less than <b>1</b>.</exception>
    public Cache(int capacity = int.MaxValue, Action<CachedItemRemovalEvent<TKey, TValue>>? removeCallback = null)
        : this( EqualityComparer<TKey>.Default, capacity, removeCallback ) { }

    /// <summary>
    /// Creates a new empty <see cref="Cache{TKey,TValue}"/> instance that uses a custom key comparer.
    /// </summary>
    /// <param name="keyComparer">Custom key equality comparer.</param>
    /// <param name="capacity">An optional maximum capacity. Equal to <see cref="Int32.MaxValue"/> by default.</param>
    /// <param name="removeCallback">An optional callback which gets invoked every time an entry is removed from this cache.</param>
    /// <exception cref="ArgumentOutOfRangeException">When <paramref name="capacity"/> is less than <b>1</b>.</exception>
    public Cache(
        IEqualityComparer<TKey> keyComparer,
        int capacity = int.MaxValue,
        Action<CachedItemRemovalEvent<TKey, TValue>>? removeCallback = null)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Capacity = capacity;
        RemoveCallback = removeCallback;
        _map = new Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>>( keyComparer );
        _order = DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>>.Empty;
    }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <summary>
    /// An optional callback which gets invoked every time an entry is removed from this cache.
    /// </summary>
    public Action<CachedItemRemovalEvent<TKey, TValue>>? RemoveCallback { get; }

    /// <inheritdoc />
    public int Count => _map.Count;

    /// <inheritdoc />
    public IEqualityComparer<TKey> Comparer => _map.Comparer;

    /// <inheritdoc />
    public KeyValuePair<TKey, TValue>? Oldest => _order.Head?.Value;

    /// <summary>
    /// Currently newest cache entry.
    /// </summary>
    public KeyValuePair<TKey, TValue>? Newest => _order.Tail?.Value;

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );

    /// <inheritdoc />
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    /// <inheritdoc cref="ICache{TKey,TValue}.this[TKey]" />
    public TValue this[TKey key]
    {
        get
        {
            var node = _map[key];
            SetNewest( node );
            return node.Value.Value;
        }
        set => AddOrUpdate( key, value );
    }

    /// <inheritdoc />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

    /// <inheritdoc />
    /// <remarks>
    /// Restarts an entry associated with the specified <paramref name="key"/>, if it exists.
    /// See <see cref="Restart(TKey)"/> for more information.
    /// </remarks>
    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        if ( ! _map.TryGetValue( key, out var node ) )
        {
            value = default;
            return false;
        }

        SetNewest( node );
        value = node.Value.Value;
        return true;
    }

    /// <inheritdoc />
    public bool TryAdd(TKey key, TValue value)
    {
        var node = CreateNode( key, value );
        if ( ! _map.TryAdd( key, node ) )
            return false;

        _order = _order.AddLast( node );
        CheckCapacity();
        return true;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Restarts an updated entry associated with the specified <paramref name="key"/>.
    /// See <see cref="Restart(TKey)"/> for more information.
    /// </remarks>
    public AddOrUpdateResult AddOrUpdate(TKey key, TValue value)
    {
        ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( exists )
        {
            var oldValue = node.Value.Value;
            node.Value = KeyValuePair.Create( key, value );
            SetNewest( node );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateReplaced( node.Value.Key, oldValue, value ) );
            return AddOrUpdateResult.Updated;
        }

        node = CreateNode( key, value );
        _order = _order.AddLast( node );
        CheckCapacity();
        return AddOrUpdateResult.Added;
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if ( ! _map.Remove( key, out var node ) )
            return false;

        _order = _order.Remove( node );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Value.Key, node.Value.Value ) );
        return true;
    }

    /// <inheritdoc />
    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        if ( ! _map.Remove( key, out var node ) )
        {
            removed = default;
            return false;
        }

        _order = _order.Remove( node );
        removed = node.Value.Value;
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Value.Key, node.Value.Value ) );
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Marks an entry associated with the specified <paramref name="key"/> as <see cref="Newest"/>, if it exists.</remarks>
    public bool Restart(TKey key)
    {
        if ( ! _map.TryGetValue( key, out var node ) )
            return false;

        SetNewest( node );
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _map.Clear();
        if ( RemoveCallback is not null )
        {
            foreach ( var (key, value) in _order )
                RemoveCallback( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( key, value ) );
        }

        _order = _order.Clear();
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _order.GetEnumerator();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static DoublyLinkedNode<KeyValuePair<TKey, TValue>> CreateNode(TKey key, TValue value)
    {
        return new DoublyLinkedNode<KeyValuePair<TKey, TValue>>( KeyValuePair.Create( key, value ) );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetNewest(DoublyLinkedNode<KeyValuePair<TKey, TValue>> node)
    {
        _order = _order.Remove( node ).AddLast( node );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void CheckCapacity()
    {
        if ( _map.Count <= Capacity )
            return;

        var node = _order.Head;
        Assume.IsNotNull( node );
        _map.Remove( node.Value.Key );
        _order = _order.Remove( node );
        Assume.ContainsExactly( _map, Capacity );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Value.Key, node.Value.Value ) );
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
