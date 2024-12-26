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
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Internal;

namespace LfrlAnvil.Collections;

/// <summary>
/// Represents a generic set of elements whose insertion order is preserved.
/// </summary>
/// <typeparam name="T">Element type.</typeparam>
public class SequentialHashSet<T> : ISet<T>, IReadOnlySet<T>
    where T : notnull
{
    private readonly Dictionary<T, int> _indexMap;
    private SparseListSlim<T> _order;

    /// <summary>
    /// Creates a new empty <see cref="SequentialHashSet{T}"/> instance with <see cref="Comparer{T}.Default"/> comparer.
    /// </summary>
    public SequentialHashSet()
        : this( EqualityComparer<T>.Default ) { }

    /// <summary>
    /// Creates a new empty <see cref="SequentialHashSet{T}"/> instance.
    /// </summary>
    /// <param name="comparer">Element comparer.</param>
    public SequentialHashSet(IEqualityComparer<T> comparer)
    {
        _indexMap = new Dictionary<T, int>( comparer );
        _order = SparseListSlim<T>.Create();
    }

    /// <inheritdoc cref="ICollection{T}.Count" />
    public int Count => _indexMap.Count;

    /// <summary>
    /// Element comparer.
    /// </summary>
    public IEqualityComparer<T> Comparer => _indexMap.Comparer;

    bool ICollection<T>.IsReadOnly => (( ICollection<KeyValuePair<T, int>> )_indexMap).IsReadOnly;

    /// <inheritdoc />
    public bool Add(T item)
    {
        ref var index = ref CollectionsMarshal.GetValueRefOrAddDefault( _indexMap, item, out var exists );
        if ( exists )
            return false;

        index = _order.Add( item );
        return true;
    }

    /// <inheritdoc />
    public bool Remove(T item)
    {
        if ( ! _indexMap.Remove( item, out var index ) )
            return false;

        _order.Remove( index );
        return true;
    }

    /// <inheritdoc cref="ICollection{T}.Contains(T)" />
    [Pure]
    public bool Contains(T item)
    {
        return _indexMap.ContainsKey( item );
    }

    /// <inheritdoc />
    public void Clear()
    {
        _indexMap.Clear();
        _order.Clear();
    }

    /// <inheritdoc />
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

    /// <inheritdoc />
    public void UnionWith(IEnumerable<T> other)
    {
        if ( ReferenceEquals( this, other ) )
            return;

        foreach ( var item in other )
            Add( item );
    }

    /// <inheritdoc />
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
        var itemsToRemove = ListSlim<T>.Create();

        foreach ( var (_, value) in _order )
        {
            if ( ! otherSet.Contains( value ) )
                itemsToRemove.Add( value );
        }

        foreach ( var item in itemsToRemove )
            Remove( item );
    }

    /// <inheritdoc />
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

    /// <inheritdoc cref="ISet{T}.Overlaps(IEnumerable{T})" />
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

    /// <inheritdoc cref="ISet{T}.SetEquals(IEnumerable{T})" />
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

    /// <inheritdoc cref="ISet{T}.IsSupersetOf(IEnumerable{T})" />
    [Pure]
    public bool IsSupersetOf(IEnumerable<T> other)
    {
        return ReferenceEquals( this, other ) || IsSupersetOf( this, other );
    }

    /// <inheritdoc cref="ISet{T}.IsProperSupersetOf(IEnumerable{T})" />
    [Pure]
    public bool IsProperSupersetOf(IEnumerable<T> other)
    {
        return Count > 0 && ! ReferenceEquals( this, other ) && IsProperSupersetOf( this, other, Comparer );
    }

    /// <inheritdoc cref="ISet{T}.IsSubsetOf(IEnumerable{T})" />
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

    /// <inheritdoc cref="ISet{T}.IsProperSubsetOf(IEnumerable{T})" />
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

    /// <summary>
    /// Creates a new <see cref="LinkedListSlimNodeEnumerator{T}"/> instance for this hash set.
    /// </summary>
    /// <returns>New <see cref="LinkedListSlimNodeEnumerator{T}"/> instance.</returns>
    [Pure]
    public LinkedListSlimNodeEnumerator<T> GetEnumerator()
    {
        return new LinkedListSlimNodeEnumerator<T>( _order.First );
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
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
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
