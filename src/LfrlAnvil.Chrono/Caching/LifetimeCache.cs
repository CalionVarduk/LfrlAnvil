using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Chrono.Caching;

public sealed class LifetimeCache<TKey, TValue> : ILifetimeCache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, DoublyLinkedNode<Entry>> _map;
    private DoublyLinkedNodeSequence<Entry> _order;

    public LifetimeCache(ITimestampProvider timestamps, Duration lifetime, int capacity = int.MaxValue)
        : this( EqualityComparer<TKey>.Default, timestamps, lifetime, capacity ) { }

    public LifetimeCache(IEqualityComparer<TKey> keyComparer, ITimestampProvider timestamps, Duration lifetime, int capacity = int.MaxValue)
    {
        Ensure.IsGreaterThan( capacity, 0 );
        Ensure.IsGreaterThan( lifetime, Duration.Zero );
        Capacity = capacity;
        Lifetime = lifetime;
        Timestamps = timestamps;
        _map = new Dictionary<TKey, DoublyLinkedNode<Entry>>( keyComparer );
        _order = DoublyLinkedNodeSequence<Entry>.Empty;
    }

    public int Capacity { get; }
    public Duration Lifetime { get; }
    public ITimestampProvider Timestamps { get; }
    public int Count => _map.Count;
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public KeyValuePair<TKey, TValue>? Oldest => _order.Head?.Value.ToKeyValuePair();
    public KeyValuePair<TKey, TValue>? Newest => _order.Tail?.Value.ToKeyValuePair();
    public IEnumerable<TKey> Keys => this.Select( static kv => kv.Key );
    public IEnumerable<TValue> Values => this.Select( static kv => kv.Value );

    public TValue this[TKey key]
    {
        get
        {
            var now = GetCurrentTimestampAndRefresh();
            var node = _map[key];
            node.Value = node.Value.Update( GetRemovalTimestamp( now ) );
            SetNewest( node );
            return node.Value.Value;
        }
        set => AddOrUpdate( key, value );
    }

    [Pure]
    public bool ContainsKey(TKey key)
    {
        Refresh();
        return _map.ContainsKey( key );
    }

    public bool TryGetValue(TKey key, [MaybeNullWhen( false )] out TValue value)
    {
        var now = GetCurrentTimestampAndRefresh();
        if ( ! _map.TryGetValue( key, out var node ) )
        {
            value = default;
            return false;
        }

        node.Value = node.Value.Update( GetRemovalTimestamp( now ) );
        SetNewest( node );
        value = node.Value.Value;
        return true;
    }

    [Pure]
    public Duration GetRemainingLifetime(TKey key)
    {
        var now = GetCurrentTimestampAndRefresh();
        return _map.TryGetValue( key, out var node ) ? node.Value.RemovalTimestamp.Subtract( now ) : Duration.Zero;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        var now = GetCurrentTimestampAndRefresh();
        var node = CreateNode( key, value, now );
        if ( ! _map.TryAdd( key, node ) )
            return false;

        _order = _order.AddLast( node );
        CheckCapacity();
        return true;
    }

    public AddOrUpdateResult AddOrUpdate(TKey key, TValue value)
    {
        var now = GetCurrentTimestampAndRefresh();
        ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( exists )
        {
            node.Value = node.Value.Update( value, GetRemovalTimestamp( now ) );
            SetNewest( node );
            return AddOrUpdateResult.Updated;
        }

        node = CreateNode( key, value, now );
        _order = _order.AddLast( node );
        CheckCapacity();
        return AddOrUpdateResult.Added;
    }

    public bool Remove(TKey key)
    {
        Refresh();
        if ( ! _map.Remove( key, out var node ) )
            return false;

        _order = _order.Remove( node );
        return true;
    }

    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        Refresh();
        if ( ! _map.Remove( key, out var node ) )
        {
            removed = default;
            return false;
        }

        _order = _order.Remove( node );
        removed = node.Value.Value;
        return true;
    }

    public bool Restart(TKey key)
    {
        var now = GetCurrentTimestampAndRefresh();
        if ( ! _map.TryGetValue( key, out var node ) )
            return false;

        node.Value = node.Value.Update( GetRemovalTimestamp( now ) );
        SetNewest( node );
        return true;
    }

    public void Refresh()
    {
        Refresh( GetCurrentTimestamp() );
    }

    public void Clear()
    {
        _map.Clear();
        _order = _order.Clear();
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        foreach ( var entry in _order )
            yield return entry.ToKeyValuePair();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private DoublyLinkedNode<Entry> CreateNode(TKey key, TValue value, Timestamp now)
    {
        return new DoublyLinkedNode<Entry>( new Entry( key, value, GetRemovalTimestamp( now ) ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Timestamp GetRemovalTimestamp(Timestamp now)
    {
        return now.Add( Lifetime );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Timestamp GetCurrentTimestamp()
    {
        var now = Timestamps.GetNow();
        if ( _order.Tail is null )
            return now;

        var last = _order.Tail.Value.RemovalTimestamp.Subtract( Lifetime );
        return now < last ? last : now;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Refresh(Timestamp now)
    {
        var node = _order.Head;
        while ( node is not null && node.Value.RemovalTimestamp <= now )
        {
            var next = node.Next;
            _map.Remove( node.Value.Key );
            _order = _order.Remove( node );
            node = next;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Timestamp GetCurrentTimestampAndRefresh()
    {
        var now = GetCurrentTimestamp();
        Refresh( now );
        return now;
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

        Assume.IsNotNull( _order.Head );
        _map.Remove( _order.Head.Value.Key );
        _order = _order.Remove( _order.Head );
        Assume.ContainsExactly( _map, Capacity );
    }

    private readonly record struct Entry(TKey Key, TValue Value, Timestamp RemovalTimestamp)
    {
        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Entry Update(Timestamp removalTimestamp)
        {
            return Update( Value, removalTimestamp );
        }

        [Pure]
        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal Entry Update(TValue value, Timestamp removalTimestamp)
        {
            return new Entry( Key, value, removalTimestamp );
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
