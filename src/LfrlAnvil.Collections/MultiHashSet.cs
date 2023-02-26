using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections;

public class MultiHashSet<T> : IMultiSet<T>
    where T : notnull
{
    private readonly Dictionary<T, int> _map;

    public MultiHashSet()
        : this( EqualityComparer<T>.Default ) { }

    public MultiHashSet(IEqualityComparer<T> comparer)
    {
        _map = new Dictionary<T, int>( comparer );
        FullCount = 0;
    }

    public long FullCount { get; private set; }
    public int Count => _map.Count;
    public IEqualityComparer<T> Comparer => _map.Comparer;
    public IEnumerable<T> DistinctItems => _map.Keys;

    public IEnumerable<T> Items
    {
        get
        {
            using var enumerator = GetEnumerator();

            while ( enumerator.MoveNext() )
            {
                var (item, multiplicity) = enumerator.Current;
                for ( var i = 0; i < multiplicity; ++i )
                    yield return item;
            }
        }
    }

    bool ICollection<Pair<T, int>>.IsReadOnly => false;

    [Pure]
    public bool Contains(T item)
    {
        return _map.ContainsKey( item );
    }

    [Pure]
    public bool Contains(T item, int multiplicity)
    {
        return GetMultiplicity( item ) >= multiplicity;
    }

    [Pure]
    public bool Contains(Pair<T, int> item)
    {
        return Contains( item.First, item.Second );
    }

    [Pure]
    public int GetMultiplicity(T item)
    {
        return _map.GetValueOrDefault( item );
    }

    public int SetMultiplicity(T item, int value)
    {
        Ensure.IsGreaterThanOrEqualTo( value, 0, nameof( value ) );

        if ( value == 0 )
        {
            if ( _map.TryGetValue( item, out var multiplicity ) )
                RemoveAllImpl( item, multiplicity );

            return multiplicity;
        }

        ref var multiplicityRef = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, item, out var exists );
        if ( exists )
        {
            var oldMultiplicity = multiplicityRef;
            FullCount += value - oldMultiplicity;
            multiplicityRef = value;
            return oldMultiplicity;
        }

        FullCount += value;
        multiplicityRef = value;
        return 0;
    }

    public int Add(T item)
    {
        return AddImpl( item, 1 );
    }

    public int AddMany(T item, int count)
    {
        Ensure.IsGreaterThan( count, 0, nameof( count ) );
        return AddImpl( item, count );
    }

    public int Remove(T item)
    {
        return RemoveImpl( item, 1 );
    }

    public int RemoveMany(T item, int count)
    {
        Ensure.IsGreaterThan( count, 0, nameof( count ) );
        return RemoveImpl( item, count );
    }

    public int RemoveAll(T item)
    {
        if ( ! _map.TryGetValue( item, out var multiplicity ) )
            return 0;

        return RemoveAllImpl( item, multiplicity );
    }

    public void Clear()
    {
        _map.Clear();
        FullCount = 0;
    }

    public void ExceptWith(IEnumerable<Pair<T, int>> other)
    {
        if ( ReferenceEquals( this, other ) )
        {
            Clear();
            return;
        }

        foreach ( var (item, count) in other )
        {
            if ( count <= 0 )
                continue;

            RemoveImpl( item, count );
        }
    }

    public void UnionWith(IEnumerable<Pair<T, int>> other)
    {
        if ( ReferenceEquals( this, other ) )
            return;

        foreach ( var (item, count) in other )
        {
            if ( count <= 0 )
                continue;

            if ( ! _map.TryGetValue( item, out var multiplicity ) )
            {
                AddNewImpl( item, count );
                continue;
            }

            if ( multiplicity >= count )
                continue;

            FullCount += count - multiplicity;
            _map[item] = count;
        }
    }

    public void IntersectWith(IEnumerable<Pair<T, int>> other)
    {
        if ( Count == 0 || ReferenceEquals( this, other ) )
            return;

        if ( other is IReadOnlyCollection<Pair<T, int>> collection && collection.Count == 0 )
        {
            Clear();
            return;
        }

        var otherSet = GetOtherSet( other, Comparer );
        var itemsToUpdate = new List<(T Item, int OldMultiplicity, int NewMultiplicity)>();

        foreach ( var (item, multiplicity) in _map )
        {
            var count = otherSet.GetMultiplicity( item );

            if ( count >= multiplicity )
                continue;

            itemsToUpdate.Add( (item, multiplicity, count) );
        }

        foreach ( var (item, oldMultiplicity, newMultiplicity) in itemsToUpdate )
        {
            if ( newMultiplicity > 0 )
            {
                FullCount -= oldMultiplicity - newMultiplicity;
                _map[item] = newMultiplicity;
                continue;
            }

            RemoveAll( item );
        }
    }

    public void SymmetricExceptWith(IEnumerable<Pair<T, int>> other)
    {
        if ( ReferenceEquals( this, other ) )
        {
            Clear();
            return;
        }

        foreach ( var (item, count) in other )
        {
            if ( count <= 0 )
                continue;

            if ( ! _map.TryGetValue( item, out var multiplicity ) )
            {
                AddNewImpl( item, count );
                continue;
            }

            var newMultiplicity = multiplicity > count
                ? multiplicity - count
                : count - multiplicity;

            if ( newMultiplicity == 0 )
            {
                RemoveAllImpl( item, count );
                continue;
            }

            FullCount -= multiplicity - newMultiplicity;
            _map[item] = newMultiplicity;
        }
    }

    [Pure]
    public bool Overlaps(IEnumerable<Pair<T, int>> other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        foreach ( var (item, count) in other )
        {
            if ( count <= 0 )
                continue;

            if ( Contains( item ) )
                return true;
        }

        return false;
    }

    [Pure]
    public bool SetEquals(IEnumerable<Pair<T, int>> other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        var otherSet = GetOtherSet( other, Comparer );

        if ( FullCount != otherSet.FullCount )
            return false;

        foreach ( var (item, count) in otherSet )
        {
            if ( GetMultiplicity( item ) != count )
                return false;
        }

        return true;
    }

    [Pure]
    public bool IsSupersetOf(IEnumerable<Pair<T, int>> other)
    {
        if ( ReferenceEquals( this, other ) )
            return true;

        var otherSet = GetOtherSet( other, Comparer );

        if ( FullCount < otherSet.FullCount )
            return false;

        foreach ( var (item, count) in otherSet )
        {
            if ( GetMultiplicity( item ) < count )
                return false;
        }

        return true;
    }

    [Pure]
    public bool IsProperSupersetOf(IEnumerable<Pair<T, int>> other)
    {
        if ( Count == 0 || ReferenceEquals( this, other ) )
            return false;

        var otherSet = GetOtherSet( other, Comparer );

        if ( FullCount <= otherSet.FullCount )
            return false;

        var equalMultiplicityCount = 0;

        foreach ( var (item, count) in otherSet )
        {
            var multiplicity = GetMultiplicity( item );

            if ( multiplicity < count )
                return false;

            if ( multiplicity == count )
                ++equalMultiplicityCount;
        }

        return equalMultiplicityCount < Count;
    }

    [Pure]
    public bool IsSubsetOf(IEnumerable<Pair<T, int>> other)
    {
        return GetOtherSet( other, Comparer ).IsSupersetOf( this );
    }

    [Pure]
    public bool IsProperSubsetOf(IEnumerable<Pair<T, int>> other)
    {
        return GetOtherSet( other, Comparer ).IsProperSupersetOf( this );
    }

    [Pure]
    public IEnumerator<Pair<T, int>> GetEnumerator()
    {
        return _map.Select( static v => Pair.Create( v.Key, v.Value ) ).GetEnumerator();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int AddImpl(T item, int count)
    {
        ref var multiplicity = ref CollectionsMarshal.GetValueRefOrAddDefault( _map, item, out var exists );
        multiplicity = exists ? checked( multiplicity + count ) : count;
        FullCount += count;
        return multiplicity;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private void AddNewImpl(T item, int count)
    {
        FullCount += count;
        _map.Add( item, count );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int RemoveImpl(T item, int count)
    {
        ref var multiplicity = ref CollectionsMarshal.GetValueRefOrNullRef( _map, item );
        if ( Unsafe.IsNullRef( ref multiplicity ) )
            return -1;

        if ( multiplicity > count )
        {
            multiplicity -= count;
            FullCount -= count;
            return multiplicity;
        }

        FullCount -= multiplicity;
        _map.Remove( item );
        return 0;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private int RemoveAllImpl(T item, int multiplicity)
    {
        FullCount -= multiplicity;
        _map.Remove( item );
        return multiplicity;
    }

    private static IMultiSet<T> GetOtherSet(IEnumerable<Pair<T, int>> other, IEqualityComparer<T> comparer)
    {
        if ( other is IMultiSet<T> otherSet && otherSet.Comparer.Equals( comparer ) )
            return otherSet;

        var result = new MultiHashSet<T>( comparer );
        foreach ( var (item, count) in other )
        {
            if ( count <= 0 )
                continue;

            result.AddImpl( item, count );
        }

        return result;
    }

    bool ISet<Pair<T, int>>.Add(Pair<T, int> item)
    {
        AddMany( item.First, item.Second );
        return true;
    }

    void ICollection<Pair<T, int>>.CopyTo(Pair<T, int>[] array, int arrayIndex)
    {
        CollectionCopying.CopyTo( this, array, arrayIndex );
    }

    void ICollection<Pair<T, int>>.Add(Pair<T, int> item)
    {
        AddMany( item.First, item.Second );
    }

    bool ICollection<Pair<T, int>>.Remove(Pair<T, int> item)
    {
        return RemoveMany( item.First, item.Second ) != -1;
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
