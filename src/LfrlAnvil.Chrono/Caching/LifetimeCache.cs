using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Caching;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Chrono.Caching;

/// <inheritdoc />
public sealed class LifetimeCache<TKey, TValue> : ILifetimeCache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DoublyLinkedNode<Entry>> _map;
    private DoublyLinkedNodeSequence<Entry> _order;

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
        _map = new Dictionary<TKey, DoublyLinkedNode<Entry>>( keyComparer );
        _order = DoublyLinkedNodeSequence<Entry>.Empty;
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
    public int Count => _map.Count;

    /// <inheritdoc />
    public IEqualityComparer<TKey> Comparer => _map.Comparer;

    /// <inheritdoc />
    public KeyValuePair<TKey, TValue>? Oldest => _order.Head?.Value.ToKeyValuePair();

    /// <summary>
    /// Currently newest cache entry.
    /// </summary>
    public KeyValuePair<TKey, TValue>? Newest => _order.Tail?.Value.ToKeyValuePair();

    /// <inheritdoc />
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );

    /// <inheritdoc />
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    /// <inheritdoc cref="ICache{TKey,TValue}.this" />
    public TValue this[TKey key]
    {
        get
        {
            var node = _map[key];
            node.Value = node.Value.Update( GetTimeOfRemoval() );
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
    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        if ( ! _map.TryGetValue( key, out var node ) )
        {
            value = default;
            return false;
        }

        node.Value = node.Value.Update( GetTimeOfRemoval() );
        SetNewest( node );
        value = node.Value.Value;
        return true;
    }

    /// <inheritdoc />
    [Pure]
    public Duration GetRemainingLifetime(TKey key)
    {
        return _map.TryGetValue( key, out var node ) ? node.Value.TimeOfRemoval.Subtract( CurrentTimestamp ) : Duration.Zero;
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
    public AddOrUpdateResult AddOrUpdate(TKey key, TValue value)
    {
        ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( exists )
        {
            var oldValue = node.Value.Value;
            node.Value = node.Value.Update( value, GetTimeOfRemoval() );
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
    public bool Restart(TKey key)
    {
        if ( ! _map.TryGetValue( key, out var node ) )
            return false;

        node.Value = node.Value.Update( GetTimeOfRemoval() );
        SetNewest( node );
        return true;
    }

    /// <inheritdoc />
    public void Move(Duration delta)
    {
        if ( delta <= Duration.Zero )
            return;

        CurrentTimestamp = CurrentTimestamp.Add( delta );

        var node = _order.Head;
        while ( node is not null && node.Value.TimeOfRemoval <= CurrentTimestamp )
        {
            var next = node.Next;
            _map.Remove( node.Value.Key );
            _order = _order.Remove( node );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Value.Key, node.Value.Value ) );
            node = next;
        }
    }

    /// <inheritdoc />
    public void Clear()
    {
        _map.Clear();
        if ( RemoveCallback is not null )
        {
            foreach ( var e in _order )
                RemoveCallback( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( e.Key, e.Value ) );
        }

        _order = _order.Clear();
    }

    /// <inheritdoc />
    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach ( var entry in _order )
            yield return entry.ToKeyValuePair();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DoublyLinkedNode<Entry> CreateNode(TKey key, TValue value)
    {
        return new DoublyLinkedNode<Entry>( new Entry( key, value, GetTimeOfRemoval() ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Timestamp GetTimeOfRemoval()
    {
        return CurrentTimestamp.Add( Lifetime );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void SetNewest(DoublyLinkedNode<Entry> node)
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

    private readonly record struct Entry(TKey Key, TValue Value, Timestamp TimeOfRemoval)
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
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
