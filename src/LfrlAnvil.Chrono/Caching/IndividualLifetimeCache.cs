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

public sealed class IndividualLifetimeCache<TKey, TValue> : IIndividualLifetimeCache<TKey, TValue>
    where TKey : notnull
{
    private readonly Dictionary<TKey, Node> _map;
    private readonly List<Node> _heap;

    public IndividualLifetimeCache(
        Timestamp startTimestamp,
        Duration lifetime,
        int capacity = int.MaxValue,
        Action<CachedItemRemovalEvent<TKey, TValue>>? removeCallback = null)
        : this( EqualityComparer<TKey>.Default, startTimestamp, lifetime, capacity, removeCallback ) { }

    public IndividualLifetimeCache(
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
        _map = new Dictionary<TKey, Node>( keyComparer );
        _heap = new List<Node>();
    }

    public int Capacity { get; }
    public Duration Lifetime { get; }
    public Timestamp StartTimestamp { get; }
    public Timestamp CurrentTimestamp { get; private set; }
    public Action<CachedItemRemovalEvent<TKey, TValue>>? RemoveCallback { get; }
    public int Count => _map.Count;
    public IEqualityComparer<TKey> Comparer => _map.Comparer;
    public KeyValuePair<TKey, TValue>? Oldest => _heap.Count > 0 ? _heap[0].ToKeyValuePair() : null;
    public IEnumerable<TKey> Keys => _heap.Select( static kv => kv.Key );
    public IEnumerable<TValue> Values => _heap.Select( static kv => kv.Value );

    public TValue this[TKey key]
    {
        get
        {
            var node = _map[key];
            Restart( node );
            return node.Value;
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

        Restart( node );
        value = node.Value;
        return true;
    }

    [Pure]
    public Duration GetRemainingLifetime(TKey key)
    {
        return _map.TryGetValue( key, out var node ) ? node.TimeOfRemoval.Subtract( CurrentTimestamp ) : Duration.Zero;
    }

    public bool TryAdd(TKey key, TValue value)
    {
        return TryAdd( key, value, Lifetime );
    }

    public bool TryAdd(TKey key, TValue value, Duration lifetime)
    {
        var node = CreateNode( key, value, lifetime );
        if ( ! _map.TryAdd( key, node ) )
            return false;

        PushToHeap( node );
        CheckCapacity();
        return true;
    }

    public AddOrUpdateResult AddOrUpdate(TKey key, TValue value)
    {
        return AddOrUpdate( key, value, Lifetime );
    }

    public AddOrUpdateResult AddOrUpdate(TKey key, TValue value, Duration lifetime)
    {
        ref var node = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, key, out var exists )!;
        if ( exists )
        {
            var oldValue = node.Value;
            node.Value = value;
            node.Lifetime = lifetime;
            Restart( node );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateReplaced( node.Key, oldValue, value ) );
            return AddOrUpdateResult.Updated;
        }

        node = CreateNode( key, value, lifetime );
        PushToHeap( node );
        CheckCapacity();
        return AddOrUpdateResult.Added;
    }

    public bool Remove(TKey key)
    {
        if ( ! _map.Remove( key, out var node ) )
            return false;

        RemoveFromHeap( node );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Key, node.Value ) );
        return true;
    }

    public bool Remove(TKey key, [MaybeNullWhen( false )] out TValue removed)
    {
        if ( ! _map.Remove( key, out var node ) )
        {
            removed = default;
            return false;
        }

        RemoveFromHeap( node );
        removed = node.Value;
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Key, node.Value ) );
        return true;
    }

    public bool Restart(TKey key)
    {
        if ( ! _map.TryGetValue( key, out var node ) )
            return false;

        Restart( node );
        return true;
    }

    public void Move(Duration delta)
    {
        CurrentTimestamp = CurrentTimestamp.Add( delta );
        while ( _heap.Count > 0 )
        {
            var node = _heap[0];
            if ( node.TimeOfRemoval > CurrentTimestamp )
                break;

            _map.Remove( node.Key );
            PopFromHeap( node );
            RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Key, node.Value ) );
        }
    }

    public void Clear()
    {
        _map.Clear();
        if ( RemoveCallback is not null )
        {
            foreach ( var n in _heap )
                RemoveCallback( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( n.Key, n.Value ) );
        }

        _heap.Clear();
    }

    [Pure]
    public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
    {
        return _heap.Select( static n => n.ToKeyValuePair() ).GetEnumerator();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void Restart(Node node)
    {
        var oldTimeOfRemoval = node.TimeOfRemoval;
        node.UpdateTimeOfRemoval( CurrentTimestamp );
        FixHeapRelative( node, oldTimeOfRemoval );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void CheckCapacity()
    {
        if ( _map.Count <= Capacity )
            return;

        var node = _heap[0];
        _map.Remove( node.Key );
        PopFromHeap( node );
        Assume.ContainsExactly( _map, Capacity );
        RemoveCallback?.Invoke( CachedItemRemovalEvent<TKey, TValue>.CreateRemoved( node.Key, node.Value ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private Node CreateNode(TKey key, TValue value, Duration lifetime)
    {
        return new Node( key, value, lifetime, CurrentTimestamp.Add( lifetime ), _heap.Count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void PushToHeap(Node node)
    {
        Assume.Equals( node.Index, _heap.Count );
        _heap.Add( node );
        FixHeapUp( node.Index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void PopFromHeap(Node node)
    {
        Assume.Equals( node, _heap[0] );
        Assume.Equals( node.Index, 0 );
        var last = _heap[^1];
        _heap[0] = last;
        last.AssignIndexFrom( node );
        _heap.RemoveAt( _heap.Count - 1 );
        FixHeapDown( 0 );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void RemoveFromHeap(Node node)
    {
        var last = _heap[^1];
        _heap[node.Index] = last;
        last.AssignIndexFrom( node );
        _heap.RemoveAt( _heap.Count - 1 );

        if ( node.Index < _heap.Count )
            FixHeapRelative( last, node.TimeOfRemoval );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixHeapRelative(Node node, Timestamp oldTimeOfRemoval)
    {
        if ( node.TimeOfRemoval > oldTimeOfRemoval )
            FixHeapDown( node.Index );
        else
            FixHeapUp( node.Index );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixHeapUp(int i)
    {
        while ( i > 0 )
        {
            var p = (i - 1) >> 1;
            var node = _heap[i];
            var parentNode = _heap[p];
            if ( parentNode.TimeOfRemoval <= node.TimeOfRemoval )
                break;

            (_heap[i], _heap[p]) = (_heap[p], _heap[i]);
            node.SwapIndexWith( parentNode );
            i = p;
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void FixHeapDown(int i)
    {
        var l = (i << 1) + 1;
        while ( l < _heap.Count )
        {
            var node = _heap[i];
            var leftNode = _heap[l];

            var r = l + 1;
            var swap = leftNode.TimeOfRemoval < node.TimeOfRemoval ? leftNode : node;
            if ( r < _heap.Count )
            {
                var rightNode = _heap[r];
                if ( rightNode.TimeOfRemoval < swap.TimeOfRemoval )
                    swap = rightNode;
            }

            if ( ReferenceEquals( node, swap ) )
                break;

            (_heap[i], _heap[swap.Index]) = (_heap[swap.Index], _heap[i]);
            node.SwapIndexWith( swap );
            i = node.Index;
            l = (i << 1) + 1;
        }
    }

    private sealed class Node
    {
        internal Node(TKey key, TValue value, Duration lifetime, Timestamp timeOfRemoval, int index)
        {
            Key = key;
            Value = value;
            Lifetime = lifetime;
            TimeOfRemoval = timeOfRemoval;
            Index = index;
        }

        internal readonly TKey Key;
        internal Duration Lifetime;
        internal TValue Value;
        internal Timestamp TimeOfRemoval { get; private set; }
        internal int Index { get; private set; }

        [Pure]
        public override string ToString()
        {
            return $"[{Index}]: {Key} => {Value} ({TimeOfRemoval})";
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void UpdateTimeOfRemoval(Timestamp now)
        {
            TimeOfRemoval = now.Add( Lifetime );
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void SwapIndexWith(Node other)
        {
            (Index, other.Index) = (other.Index, Index);
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        internal void AssignIndexFrom(Node other)
        {
            Index = other.Index;
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
