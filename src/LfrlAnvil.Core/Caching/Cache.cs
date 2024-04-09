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

public sealed class Cache<TKey, TValue> : ICache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DoublyLinkedNode<KeyValuePair<TKey, TValue>>> _map;
    private DoublyLinkedNodeSequence<KeyValuePair<TKey, TValue>> _order;

    public Cache(int capacity = int.MaxValue, Action<CachedItemRemovalEvent<TKey, TValue>>? removeCallback = null)
        : this( EqualityComparer<TKey>.Default, capacity, removeCallback ) { }

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

    public int Capacity { get; }
    public Action<CachedItemRemovalEvent<TKey, TValue>>? RemoveCallback { get; }
    public int Count => _map.Count;
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public KeyValuePair<TKey, TValue>? Oldest => _order.Head?.Value;
    public KeyValuePair<TKey, TValue>? Newest => _order.Tail?.Value;
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

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

    [Pure]
    public bool ContainsKey(TKey key)
    {
        return _map.ContainsKey( key );
    }

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

    public bool TryAdd(TKey key, TValue value)
    {
        var node = CreateNode( key, value );
        if ( ! _map.TryAdd( key, node ) )
            return false;

        _order = _order.AddLast( node );
        CheckCapacity();
        return true;
    }

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

    public bool Remove(TKey key)
    {
        if ( ! _map.Remove( key, out var node ) )
            return false;

        _order = _order.Remove( node );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Value.Key, node.Value.Value ) );
        return true;
    }

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

    public bool Restart(TKey key)
    {
        if ( ! _map.TryGetValue( key, out var node ) )
            return false;

        SetNewest( node );
        return true;
    }

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
