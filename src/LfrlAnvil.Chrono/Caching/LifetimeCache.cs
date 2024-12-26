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
using LfrlAnvil.Caching;

namespace LfrlAnvil.Chrono.Caching;

/// <inheritdoc cref="ILifetimeCache{TKey,TValue}" />
public sealed class LifetimeCache<TKey, TValue> : ILifetimeCache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, int> _keyIndexMap;
    private SparseListSlim<Entry> _order;

    /// <summary>
    /// Creates a new <see cref="LifetimeCache{TKey,TValue}"/> instance that uses
    /// the <see cref="EqualityComparer{T}.Default"/> key comparer.
    /// </summary>
    /// <param name="startTimestamp"><see cref="Timestamp"/> of the creation of this cache.</param>
    /// <param name="lifetime">Lifetime of added entries.</param>
    /// <param name="capacity">An optional maximum capacity. Equal to <see cref="Int32.MaxValue"/> by default.</param>
    /// <param name="removeCallback">An optional callback which gets invoked every time an entry is removed from this cache.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="capacity"/> is less than <b>1</b> or when <paramref name="lifetime"/> is less than <b>1 tick</b>.
    /// </exception>
    public LifetimeCache(
        Timestamp startTimestamp,
        Duration lifetime,
        int capacity = int.MaxValue,
        Action<CachedItemRemovalEvent<TKey, TValue>>? removeCallback = null)
        : this( EqualityComparer<TKey>.Default, startTimestamp, lifetime, capacity, removeCallback ) { }

    /// <summary>
    /// Creates a new <see cref="LifetimeCache{TKey,TValue}"/> instance.
    /// </summary>
    /// <param name="keyComparer">Custom key equality comparer.</param>
    /// <param name="startTimestamp"><see cref="Timestamp"/> of the creation of this cache.</param>
    /// <param name="lifetime">Lifetime of added entries.</param>
    /// <param name="capacity">An optional maximum capacity. Equal to <see cref="Int32.MaxValue"/> by default.</param>
    /// <param name="removeCallback">An optional callback which gets invoked every time an entry is removed from this cache.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="capacity"/> is less than <b>1</b> or when <paramref name="lifetime"/> is less than <b>1 tick</b>.
    /// </exception>
    public LifetimeCache(
        IEqualityComparer<TKey> keyComparer,
        Timestamp startTimestamp,
        Duration lifetime,
        int capacity = int.MaxValue,
        Action<CachedItemRemovalEvent<TKey, TValue>>? removeCallback = null)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Ensure.IsGreaterThan( lifetime, Duration.Zero );
        Capacity = capacity;
        Lifetime = lifetime;
        StartTimestamp = startTimestamp;
        CurrentTimestamp = startTimestamp;
        RemoveCallback = removeCallback;
        _keyIndexMap = new Dictionary<TKey, int>( keyComparer );
        _order = SparseListSlim<Entry>.Create();
    }

    /// <inheritdoc />
    public int Capacity { get; }

    /// <inheritdoc />
    public Duration Lifetime { get; }

    /// <inheritdoc />
    public Timestamp StartTimestamp { get; }

    /// <inheritdoc />
    public Timestamp CurrentTimestamp { get; private set; }

    /// <summary>
    /// An optional callback which gets invoked every time an entry is removed from this cache.
    /// </summary>
    public Action<CachedItemRemovalEvent<TKey, TValue>>? RemoveCallback { get; }

    /// <inheritdoc />
    public int Count => _keyIndexMap.Count;

    /// <inheritdoc />
    public IEqualityComparer<TKey> Comparer => _keyIndexMap.Comparer;

    /// <inheritdoc />
    public KeyValuePair<TKey, TValue>? Oldest => _order.First?.Value.ToKeyValuePair();

    /// <summary>
    /// Currently newest cache entry.
    /// </summary>
    public KeyValuePair<TKey, TValue>? Newest => _order.Last?.Value.ToKeyValuePair();

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );

    /// <inheritdoc />
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    /// <inheritdoc cref="ICache{TKey,TValue}.this" />
    public TValue this[TKey key]
    {
        get
        {
            var index = _keyIndexMap[key];
            ref var entry = ref _order[index];
            entry = entry.Update( GetTimeOfRemoval() );
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
    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        if ( ! _keyIndexMap.TryGetValue( key, out var index ) )
        {
            value = default;
            return false;
        }

        ref var entry = ref _order[index];
        entry = entry.Update( GetTimeOfRemoval() );
        SetNewest( index, entry );
        value = entry.Value;
        return true;
    }

    /// <inheritdoc />
    [Pure]
    public Duration GetRemainingLifetime(TKey key)
    {
        if ( ! _keyIndexMap.TryGetValue( key, out var index ) )
            return Duration.Zero;

        ref var entry = ref _order[index];
        return entry.TimeOfRemoval.Subtract( CurrentTimestamp );
    }

    /// <inheritdoc />
    public bool TryAdd(TKey key, TValue value)
    {
        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( _keyIndexMap, key, out var exists );
        if ( exists )
            return false;

        index = _order.Add( new Entry( key, value, GetTimeOfRemoval() ) );
        CheckCapacity();
        return true;
    }

    /// <inheritdoc />
    public AddOrUpdateResult AddOrUpdate(TKey key, TValue value)
    {
        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( _keyIndexMap, key, out var exists );
        if ( exists )
        {
            ref var entry = ref _order[index];
            var oldValue = entry.Value;
            entry = entry.Update( value, GetTimeOfRemoval() );
            SetNewest( index, entry );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateReplaced( entry.Key, oldValue, value ) );
            return AddOrUpdateResult.Updated;
        }

        index = _order.Add( new Entry( key, value, GetTimeOfRemoval() ) );
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
    public bool Restart(TKey key)
    {
        if ( ! _keyIndexMap.TryGetValue( key, out var index ) )
            return false;

        ref var entry = ref _order[index];
        entry = entry.Update( GetTimeOfRemoval() );
        SetNewest( index, entry );
        return true;
    }

    /// <inheritdoc />
    public void Move(Duration delta)
    {
        if ( delta <= Duration.Zero )
            return;

        CurrentTimestamp = CurrentTimestamp.Add( delta );

        var node = _order.First;
        while ( node is not null && node.Value.Value.TimeOfRemoval <= CurrentTimestamp )
        {
            var next = node.Value.Next;
            var entry = node.Value.Value;
            _keyIndexMap.Remove( entry.Key );
            _order.Remove( node.Value.Index );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( entry.Key, entry.Value ) );
            node = next;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        _keyIndexMap.Clear();
        if ( RemoveCallback is not null )
        {
            foreach ( var (_, e) in _order )
                RemoveCallback( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( e.Key, e.Value ) );
        }

        _order.Clear();
    }

    /// <summary>
    /// Creates a new <see cref="Enumerator"/> instance for this cache.
    /// </summary>
    /// <returns>New <see cref="Enumerator"/> instance.</returns>
    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _order );
    }

    /// <summary>
    /// Lightweight enumerator implementation for <see cref="LifetimeCache{TKey,TValue}"/>.
    /// </summary>
    public struct Enumerator : IEnumerator<KeyValuePair<TKey, TValue>>
    {
        private LinkedListSlimNodeEnumerator<Entry> _internal;

        internal Enumerator(SparseListSlim<Entry> items)
        {
            _internal = new LinkedListSlimNodeEnumerator<Entry>( items.First );
        }

        /// <inheritdoc />
        public KeyValuePair<TKey, TValue> Current => _internal.Current.ToKeyValuePair();

        object IEnumerator.Current => Current;

        /// <inheritdoc />
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _internal.MoveNext();
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _internal.Dispose();
        }

        void IEnumerator.Reset()
        {
            (( IEnumerator )_internal).Reset();
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Timestamp GetTimeOfRemoval()
    {
        return CurrentTimestamp.Add( Lifetime );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetNewest(int index, Entry entry)
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

    internal readonly record struct Entry(TKey Key, TValue Value, Timestamp TimeOfRemoval)
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Entry Update(Timestamp timeOfRemoval)
        {
            return Update( Value, timeOfRemoval );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Entry Update(TValue value, Timestamp timeOfRemoval)
        {
            return new Entry( Key, value, timeOfRemoval );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal KeyValuePair<TKey, TValue> ToKeyValuePair()
        {
            return KeyValuePair.Create( Key, Value );
        }
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
