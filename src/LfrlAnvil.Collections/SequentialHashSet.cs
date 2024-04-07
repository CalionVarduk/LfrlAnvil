﻿using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Internal;

namespace LfrlAnvil.Collections;

public class SequentialHashSet<T> : ISet<T>, IReadOnlySet<T>
    where T : notnull
{
    private readonly Dictionary<T, DoublyLinkedNode<T>> _map;
    private DoublyLinkedNodeSequence<T> _order;

    public SequentialHashSet()
        : this( EqualityComparer<T>.Default ) { }

    public SequentialHashSet(IEqualityComparer<T> comparer)
    {
        _map = new Dictionary<T, DoublyLinkedNode<T>>( comparer );
        _order = DoublyLinkedNodeSequence<T>.Empty;
    }

    public int Count => _map.Count;
    public IEqualityComparer<T> Comparer => _map.Comparer;

    bool ICollection<T>.IsReadOnly => (( ICollection<KeyValuePair<T, DoublyLinkedNode<T>>> )_map).IsReadOnly;

    public bool Add(T item)
    {
        var node = new DoublyLinkedNode<T>( item );
        if ( ! _map.TryAdd( item, node ) )
            return false;

        _order = _order.AddLast( node );
        return true;
    }

    public bool Remove(T item)
    {
        if ( ! _map.Remove( item, out var node ) )
            return false;

        _order = _order.Remove( node );
        return true;
    }

    [Pure]
    public bool Contains(T item)
    {
        return _map.ContainsKey( item );
    }

    public void Clear()
    {
        _map.Clear();
        _order = _order.Clear();
    }

    public void ExceptWith(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
        {
            Clear();
            return;
        }

        foreach ( var item in other )
            Remove( item );
    }

    public void UnionWith(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
            return;

        foreach ( var item in other )
            Add( item );
    }

    public void IntersectWith(IEnumerable<T> other)
    {
        if ( Count == 0 || ReferenceEquals( this, other ) )
            return;

        if ( other is IReadOnlyCollection<T> collection && collection.Count == 0 )
        {
            Clear();
            return;
        }

        var otherSet = GetOtherSet( other, Comparer );
        var itemsToRemove = new List<T>();

        foreach ( var value in _order )
        {
            if ( ! otherSet.Contains( value ) )
                itemsToRemove.Add( value );
        }

        foreach ( var item in itemsToRemove )
            Remove( item );
    }

    public void SymmetricExceptWith(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
        {
            Clear();
            return;
        }

        foreach ( var item in other )
        {
            if ( ! Remove( item ) )
                Add( item );
        }
    }

    [Pure]
    public bool Overlaps(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        foreach ( var item in other )
        {
            if ( Contains( item ) )
                return true;
        }

        return false;
    }

    [Pure]
    public bool SetEquals(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        if ( other is HashSet<T> hashSet && hashSet.Comparer.Equals( Comparer ) )
            return SetEquals( this, hashSet );

        if ( other is SequentialHashSet<T> seqSet && seqSet.Comparer.Equals( Comparer ) )
            return SetEquals( this, seqSet );

        var otherSet = new HashSet<T>( Comparer );

        foreach ( var item in other )
        {
            if ( ! Contains( item ) )
                return false;

            otherSet.Add( item );
        }

        return Count == otherSet.Count;
    }

    [Pure]
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return ReferenceEquals( this, other ) || IsSupersetOf( this, other );
    }

    [Pure]
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return Count > 0 && ! ReferenceEquals( this, other ) && IsProperSupersetOf( this, other, Comparer );
    }

    [Pure]
    public bool IsSubsetOf(IEnumerable<T> other)
    {
        if ( Count == 0 || ReferenceEquals( this, other ) )
            return true;

        if ( other is IReadOnlyCollection<T> collection && collection.Count == 0 )
            return false;

        var otherSet = GetOtherSet( other, Comparer );
        return IsSupersetOf( otherSet, this );
    }

    [Pure]
    public bool IsProperSubsetOf(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
            return false;

        if ( other is IReadOnlyCollection<T> collection )
        {
            if ( collection.Count == 0 )
                return false;

            if ( Count == 0 )
                return true;
        }

        var otherSet = GetOtherSet( other, Comparer );
        return IsProperSupersetOf( otherSet, this );
    }

    [Pure]
    public IEnumerator<T> GetEnumerator()
    {
        return _order.GetEnumerator();
    }

    [Pure]
    private static IReadOnlySet<T> GetOtherSet(IEnumerable<T> other, IEqualityComparer<T> comparer)
    {
        if ( other is HashSet<T> hSet && hSet.Comparer.Equals( comparer ) )
            return hSet;

        if ( other is SequentialHashSet<T> sSet && sSet.Comparer.Equals( comparer ) )
            return sSet;

        return new HashSet<T>( other, comparer );
    }

    [Pure]
    private static bool IsSupersetOf(IReadOnlySet<T> set, IEnumerable<T> other)
    {
        foreach ( var item in other )
        {
            if ( ! set.Contains( item ) )
                return false;
        }

        return true;
    }

    [Pure]
    private static bool IsProperSupersetOf(IReadOnlySet<T> set, IEnumerable<T> other, IEqualityComparer<T> comparer)
    {
        var otherSet = new HashSet<T>( comparer );

        foreach ( var item in other )
        {
            if ( ! set.Contains( item ) )
                return false;

            otherSet.Add( item );
        }

        return set.Count > otherSet.Count;
    }

    [Pure]
    private static bool IsProperSupersetOf(IReadOnlySet<T> set, IReadOnlySet<T> other)
    {
        foreach ( var item in other )
        {
            if ( ! set.Contains( item ) )
                return false;
        }

        return set.Count > other.Count;
    }

    [Pure]
    private static bool SetEquals(SequentialHashSet<T> set, IReadOnlySet<T> other)
    {
        foreach ( var item in other )
        {
            if ( ! set.Contains( item ) )
                return false;
        }

        return set.Count == other.Count;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    void ICollection<T>.Add(T item)
    {
        Add( item );
    }

    void ICollection<T>.CopyTo(T[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }
}
