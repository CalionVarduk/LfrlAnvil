using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlSoft.NET.Core.Extensions;

namespace LfrlSoft.NET.Core.Collections
{
    public class MultiSet<T> : IMultiSet<T>
        where T : notnull
    {
        private readonly Dictionary<T, Ref<int>> _map;

        public MultiSet()
            : this( EqualityComparer<T>.Default ) { }

        public MultiSet(IEqualityComparer<T> comparer)
        {
            _map = new Dictionary<T, Ref<int>>( comparer );
            FullCount = 0;
        }

        public long FullCount { get; private set; }
        public int Count => _map.Count;
        public IEqualityComparer<T> Comparer => _map.Comparer;
        public IEnumerable<T> DistinctItems => _map.Keys;

        bool ICollection<Pair<T, int>>.IsReadOnly => false;

        [Pure]
        public bool Contains(T item)
        {
            return _map.ContainsKey( item );
        }

        [Pure]
        public int GetMultiplicity(T item)
        {
            return _map.TryGetValue( item, out var multiplicity ) ? multiplicity.Value : 0;
        }

        public int SetMultiplicity(T item, int value)
        {
            Assert.IsGreaterThanOrEqualTo( value, 0, nameof( value ) );

            if ( _map.TryGetValue( item, out var multiplicity ) )
            {
                if ( value == 0 )
                    return RemoveAllImpl( item, multiplicity );

                var oldMultiplicity = multiplicity.Value;
                FullCount = checked( FullCount + value - oldMultiplicity );
                multiplicity.Value = value;
                return oldMultiplicity;
            }

            if ( value == 0 )
                return 0;

            return AddNewImpl( item, value );
        }

        public int Add(T item)
        {
            return AddImpl( item, 1 );
        }

        public int AddMany(T item, int count)
        {
            Assert.IsGreaterThan( count, 0, nameof( count ) );
            return AddImpl( item, count );
        }

        public int Remove(T item)
        {
            return RemoveImpl( item, 1 );
        }

        public int RemoveMany(T item, int count)
        {
            Assert.IsGreaterThan( count, 0, nameof( count ) );
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

        [Pure]
        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            if ( ReferenceEquals( this, other ) )
                return false;

            var otherSet = GetOrCreateMultiSet( other );

            foreach ( var (item, multiplicity) in _map )
            {
                var otherMultiplicity = otherSet.GetMultiplicity( item );
                if ( otherMultiplicity < multiplicity.Value )
                    return false;
            }

            foreach ( var (item, multiplicity) in _map )
            {
                var otherMultiplicity = otherSet.GetMultiplicity( item );
                if ( otherMultiplicity != multiplicity.Value )
                    return true;
            }

            return false;
        }

        private IReadOnlyMultiSet<T> GetOrCreateMultiSet(IEnumerable<T> other)
        {
            if ( other is IReadOnlyMultiSet<T> multi && ReferenceEquals( Comparer, multi.Comparer ) )
                return multi;

            var otherSet = new MultiSet<T>( Comparer );
            foreach ( var item in other )
                otherSet.Add( item );

            return otherSet;
        }

        [Pure]
        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            var otherSet = new MultiSet<T>( Comparer );

            foreach ( var item in other )
            {
                if ( ! Contains( item ) )
                    return false;

                otherSet.Add( item );
            }

            foreach ( var (item, otherMultiplicity) in otherSet )
            {
                if ( GetMultiplicity( item ) < otherMultiplicity )
                    return false;
            }

            foreach ( var (item, otherMultiplicity) in otherSet )
            {
                if ( GetMultiplicity( item ) != otherMultiplicity )
                    return true;
            }

            return false;
        }

        [Pure]
        public bool IsSubsetOf(IEnumerable<T> other)
        {
            if ( ReferenceEquals( this, other ) )
                return true;

            var otherSet = new MultiSet<T>( Comparer );
            foreach ( var item in other )
                otherSet.Add( item );

            foreach ( var (item, multiplicity) in _map )
            {
                var otherMultiplicity = otherSet.GetMultiplicity( item );
                if ( otherMultiplicity < multiplicity.Value )
                    return false;
            }

            return true;
        }

        [Pure]
        public bool IsSupersetOf(IEnumerable<T> other)
        {
            if ( ReferenceEquals( this, other ) )
                return true;

            var otherSet = new MultiSet<T>( Comparer );

            foreach ( var item in other )
            {
                if ( ! Contains( item ) )
                    return false;

                otherSet.Add( item );
            }

            foreach ( var (item, otherMultiplicity) in otherSet )
            {
                if ( GetMultiplicity( item ) < otherMultiplicity )
                    return false;
            }

            return false;
        }

        [Pure]
        public bool Overlaps(IEnumerable<T> other)
        {
            return other.Any( Contains );
        }

        [Pure]
        public bool SetEquals(IEnumerable<T> other)
        {
            var otherSet = new MultiSet<T>( Comparer );

            foreach ( var item in other )
            {
                if ( ! Contains( item ) )
                    return false;

                otherSet.Add( item );
            }

            if ( Count != otherSet.Count )
                return false;

            foreach ( var (item, multiplicity) in _map )
            {
                if ( multiplicity.Value != otherSet.GetMultiplicity( item ) )
                    return false;
            }

            return true;
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach ( var item in other )
                Remove( item );
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            var otherSet = new MultiSet<T>( Comparer );
            foreach ( var item in other )
                otherSet.Add( item );

            var itemsToRemove = new List<T>();

            foreach ( var (item, multiplicity) in _map )
            {
                var otherMultiplicity = otherSet.GetMultiplicity( item );
                if ( otherMultiplicity == 0 )
                {
                    itemsToRemove.Add( item );
                    continue;
                }

                if ( otherMultiplicity >= multiplicity.Value )
                    continue;

                FullCount -= multiplicity.Value - otherMultiplicity;
                multiplicity.Value = otherMultiplicity;
            }

            foreach ( var item in itemsToRemove )
                RemoveAll( item );
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            var otherSet = new MultiSet<T>( Comparer );
            foreach ( var item in other )
                otherSet.Add( item );

            foreach ( var (item, otherMultiplicity) in otherSet )
            {
                if ( ! Contains( item ) )
                {
                    AddImpl( item, otherMultiplicity );
                    continue;
                }

                RemoveAll( item );
            }
        }

        public void UnionWith(IEnumerable<T> other)
        {
            var otherSet = new MultiSet<T>( Comparer );
            foreach ( var item in other )
                otherSet.Add( item );

            foreach ( var (item, otherMultiplicity) in otherSet )
            {
                if ( ! _map.TryGetValue( item, out var multiplicity ) )
                {
                    AddImpl( item, otherMultiplicity );
                    continue;
                }

                if ( otherMultiplicity <= multiplicity.Value )
                    continue;

                FullCount += otherMultiplicity - multiplicity.Value;
                multiplicity.Value = otherMultiplicity;
            }
        }

        [Pure]
        public IEnumerator<Pair<T, int>> GetEnumerator()
        {
            return _map.Select( v => Pair.Create( v.Key, v.Value.Value ) ).GetEnumerator();
        }

        private int AddImpl(T item, int count)
        {
            if ( ! _map.TryGetValue( item, out var multiplicity ) )
                return AddNewImpl( item, count );

            multiplicity.Value = checked( multiplicity.Value + count );
            FullCount += count;
            return multiplicity.Value;
        }

        private int AddNewImpl(T item, int count)
        {
            FullCount += count;
            _map.Add( item, Ref.Create( count ) );
            return count;
        }

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

        private int RemoveAllImpl(T item, int multiplicity)
        {
            FullCount -= multiplicity;
            _map.Remove( item );
            return multiplicity;
        }

        void ICollection<Pair<T, int>>.CopyTo(Pair<T, int>[] array, int arrayIndex)
        {
            var count = Math.Min( FullCount, array.Length - arrayIndex );
            var maxArrayIndex = arrayIndex + count - 1;

            using var enumerator = GetEnumerator();
            var index = arrayIndex;

            while ( enumerator.MoveNext() && index <= maxArrayIndex )
                array[index++] = enumerator.Current!;
        }

        void ICollection<Pair<T, int>>.Add(Pair<T, int> item)
        {
            AddMany( item.First, item.Second );
        }

        bool ICollection<Pair<T, int>>.Remove(Pair<T, int> item)
        {
            return RemoveMany( item.First, item.Second ) != -1;
        }

        bool ICollection<Pair<T, int>>.Contains(Pair<T, int> item)
        {
            return GetMultiplicity( item.First ) == item.Second;
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
