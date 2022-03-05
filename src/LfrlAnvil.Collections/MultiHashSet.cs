using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections
{
    public class MultiHashSet<T> : IMultiSet<T>
        where T : notnull
    {
        private readonly Dictionary<T, Ref<int>> _map;

        public MultiHashSet()
            : this( EqualityComparer<T>.Default ) { }

        public MultiHashSet(IEqualityComparer<T> comparer)
        {
            _map = new Dictionary<T, Ref<int>>( comparer );
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
            return _map.TryGetValue( item, out var multiplicity ) ? multiplicity.Value : 0;
        }

        public int SetMultiplicity(T item, int value)
        {
            Ensure.IsGreaterThanOrEqualTo( value, 0, nameof( value ) );

            if ( _map.TryGetValue( item, out var multiplicity ) )
            {
                if ( value == 0 )
                    return RemoveAllImpl( item, multiplicity.Value );

                var oldMultiplicity = multiplicity.Value;
                FullCount += value - oldMultiplicity;
                multiplicity.Value = value;
                return oldMultiplicity;
            }

            if ( value > 0 )
                AddNewImpl( item, value );

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

            return RemoveAllImpl( item, multiplicity.Value );
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

                if ( multiplicity.Value >= count )
                    continue;

                FullCount += count - multiplicity.Value;
                multiplicity.Value = count;
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
            var itemsToRemove = new List<T>();

            foreach ( var (item, multiplicity) in _map )
            {
                var count = otherSet.GetMultiplicity( item );

                if ( count >= multiplicity.Value )
                    continue;

                if ( count > 0 )
                {
                    FullCount -= multiplicity.Value - count;
                    multiplicity.Value = count;
                    continue;
                }

                itemsToRemove.Add( item );
            }

            foreach ( var item in itemsToRemove )
                RemoveAll( item );
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

                var newMultiplicity = multiplicity.Value > count
                    ? multiplicity.Value - count
                    : count - multiplicity.Value;

                if ( newMultiplicity == 0 )
                {
                    RemoveAllImpl( item, count );
                    continue;
                }

                FullCount -= multiplicity.Value - newMultiplicity;
                multiplicity.Value = newMultiplicity;
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
            return _map.Select( v => Pair.Create( v.Key, v.Value.Value ) ).GetEnumerator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int AddImpl(T item, int count)
        {
            if ( ! _map.TryGetValue( item, out var multiplicity ) )
                return AddNewImpl( item, count );

            multiplicity.Value = checked( multiplicity.Value + count );
            FullCount += count;
            return multiplicity.Value;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int AddNewImpl(T item, int count)
        {
            FullCount += count;
            _map.Add( item, Ref.Create( count ) );
            return count;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        private int RemoveImpl(T item, int count)
        {
            if ( ! _map.TryGetValue( item, out var multiplicity ) )
                return -1;

            if ( multiplicity.Value > count )
            {
                FullCount -= count;
                multiplicity.Value -= count;
                return multiplicity.Value;
            }

            FullCount -= multiplicity.Value;
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
}
