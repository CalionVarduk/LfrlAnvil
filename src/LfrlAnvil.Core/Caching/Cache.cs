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

namespace LfrlAnvil.Caching;

/// <inheritdoc cref="ICache{TKey,TValue}" />
/// <remarks>New entries added to this cache are added as <see cref="Newest"/>.</remarks>
public sealed class Cache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _keyIndexMap;
    private SparseListSlim<KeyValuePair<TKey, TValue>> _order;

    /// <summary>
    /// Creates a new empty <see cref="Cache{TKey,TValue}"/> instance that uses the <see cref="EqualityComparer{T}.Default"/> key comparer.
    /// </summary>
    /// <param name="capacity">An optional maximum capacity. Equal to <see cref="int.MaxValue"/> by default.</param>
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
        _keyIndexMap = new Dictionary<TKey, int>( keyComparer );
        _order = SparseListSlim<KeyValuePair<TKey, TValue>>.Create();
    }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <summary>
    /// An optional callback which gets invoked every time an entry is removed from this cache.
    /// </summary>
    public Action<CachedItemRemovalEvent<TKey, TValue>>? RemoveCallback { get; }

    /// <inheritdoc />
    public int Count => _keyIndexMap.Count;

    /// <inheritdoc />
    public IEqualityComparer<TKey> Comparer => _keyIndexMap.Comparer;

    /// <inheritdoc />
    public KeyValuePair<TKey, TValue>? Oldest => _order.First?.Value;

    /// <summary>
    /// Currently newest cache entry.
    /// </summary>
    public KeyValuePair<TKey, TValue>? Newest => _order.Last?.Value;

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );

    /// <inheritdoc />
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    /// <inheritdoc cref="ICache{TKey,TValue}.this[TKey]" />
    public TValue this[TKey key]
    {
        get
        {
            var index = _keyIndexMap[key];
            ref var entry = ref _order[index];
            SetNewest( index, entry );
            return entry.Value;
        }
        set => AddOrUpdate( key, value );
    }

    /// <inheritdoc />
    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _keyIndexMap.ContainsKey( key );
    }

    /// <inheritdoc />
    /// <remarks>
    /// Restarts an entry associated with the specified <paramref name="key"/>, if it exists.
    /// See <see cref="Restart(TKey)"/> for more information.
    /// </remarks>
    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        if ( ! _keyIndexMap.TryGetValue( key, out var index ) )
        {
            value = default;
            return false;
        }

        ref var entry = ref _order[index];
        SetNewest( index, entry );
        value = entry.Value;
        return true;
    }

    /// <inheritdoc />
    public bool TryAdd(TKey key, TValue value)
    {
        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( _keyIndexMap, key, out var exists );
        if ( exists )
            return false;

        index = _order.Add( KeyValuePair.Create( key, value ) );
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
        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( _keyIndexMap, key, out var exists );
        if ( exists )
        {
            ref var entry = ref _order[index];
            var oldValue = entry.Value;
            entry = KeyValuePair.Create( key, value );
            SetNewest( index, entry );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateReplaced( entry.Key, oldValue, value ) );
            return AddOrUpdateResult.Updated;
        }

        index = _order.Add( KeyValuePair.Create( key, value ) );
        CheckCapacity();
        return AddOrUpdateResult.Added;
    }

    /// <inheritdoc />
    public bool Remove(TKey key)
    {
        if ( ! _keyIndexMap.Remove( key, out var index ) )
            return false;

        var entry = _order[index];
        _order.Remove( index );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( entry.Key, entry.Value ) );
        return true;
    }

    /// <inheritdoc />
    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        if ( ! _keyIndexMap.Remove( key, out var index ) )
        {
            removed = default;
            return false;
        }

        var entry = _order[index];
        _order.Remove( index );
        removed = entry.Value;
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( entry.Key, entry.Value ) );
        return true;
    }

    /// <inheritdoc />
    /// <remarks>Marks an entry associated with the specified <paramref name="key"/> as <see cref="Newest"/>, if it exists.</remarks>
    public bool Restart(TKey key)
    {
        if ( ! _keyIndexMap.TryGetValue( key, out var index ) )
            return false;

        ref var entry = ref _order[index];
        SetNewest( index, entry );
        return true;
    }

    /// <inheritdoc />
    public void Clear()
    {
        _keyIndexMap.Clear();
        if ( RemoveCallback is not null )
        {
            foreach ( var (_, entry) in _order )
                RemoveCallback( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( entry.Key, entry.Value ) );
        }

        _order.Clear();
    }

    /// <summary>
    /// Creates a new <see cref="SparseListSlimNodeEnumerator{T}"/> instance for this cache.
    /// </summary>
    /// <returns>New <see cref="SparseListSlimNodeEnumerator{T}"/> instance.</returns>
    [Pure]
    public SparseListSlimNodeEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return new SparseListSlimNodeEnumerator<KeyValuePair<TKey, TValue>>( _order );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetNewest(int index, KeyValuePair<TKey, TValue> entry)
    {
        _order.Remove( index );
        _order.Add( entry );
        Assume.Equals( index, _order.Last?.Index ?? -1 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void CheckCapacity()
    {
        if ( _keyIndexMap.Count <= Capacity )
            return;

        var node = _order.First;
        Assume.IsNotNull( node );
        var entry = node.Value.Value;
        _keyIndexMap.Remove( entry.Key );
        _order.Remove( node.Value.Index );
        Assume.ContainsExactly( _keyIndexMap, Capacity );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( entry.Key, entry.Value ) );
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
}
