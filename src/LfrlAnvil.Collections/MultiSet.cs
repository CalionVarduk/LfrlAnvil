using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections
{
    // TODO: add methods for calculating Unions, Intersections etc. between 2 MultiSets
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

        void ICollection<Pair<T, int>>.CopyTo(Pair<T, int>[] array, int arrayIndex)
        {
            var count = Math.Min( Count, array.Length - arrayIndex );
            var maxArrayIndex = arrayIndex + count - 1;

            if ( maxArrayIndex < 0 )
                return;

            using var enumerator = GetEnumerator();
            var index = arrayIndex;

            while ( index < 0 && enumerator.MoveNext() )
                ++index;

            while ( enumerator.MoveNext() && index <= maxArrayIndex )
                array[index++] = enumerator.Current;
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
