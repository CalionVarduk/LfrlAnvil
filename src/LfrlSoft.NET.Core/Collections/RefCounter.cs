using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace LfrlSoft.NET.Core.Collections
{
    public class RefCounter<TKey> : IRefCounter<TKey>
        where TKey : notnull
    {
        private readonly Dictionary<TKey, Ref<int>> _map;

        public RefCounter()
            : this( EqualityComparer<TKey>.Default ) { }

        public RefCounter(IEqualityComparer<TKey> comparer)
        {
            _map = new Dictionary<TKey, Ref<int>>( comparer );
        }

        public int this[TKey key] => _map[key].Value;
        public int Count => _map.Count;
        public IEnumerable<TKey> Keys => _map.Keys;
        public IEnumerable<int> Values => _map.Values.Select( v => v.Value );
        public IEqualityComparer<TKey> Comparer => _map.Comparer;

        public int Increment(TKey key)
        {
            if ( _map.TryGetValue( key, out var @ref ) )
                return checked( ++@ref.Value );

            _map.Add( key, Ref.Create( 1 ) );
            return 1;
        }

        public int IncrementBy(TKey key, int count)
        {
            Assert.IsGreaterThan( count, 0, nameof( count ) );

            if ( _map.TryGetValue( key, out var @ref ) )
                return checked( @ref.Value += count );

            _map.Add( key, Ref.Create( count ) );
            return count;
        }

        public int Decrement(TKey key)
        {
            if ( ! _map.TryGetValue( key, out var @ref ) )
                return -1;

            if ( @ref.Value > 1 )
                return --@ref.Value;

            _map.Remove( key );
            return 0;
        }

        public int DecrementBy(TKey key, int count)
        {
            Assert.IsGreaterThan( count, 0, nameof( count ) );

            if ( ! _map.TryGetValue( key, out var @ref ) )
                return -1;

            if ( @ref.Value > count )
                return @ref.Value -= count;

            _map.Remove( key );
            return 0;
        }

        public bool Remove(TKey key)
        {
            return _map.Remove( key );
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool ContainsKey(TKey key)
        {
            return _map.ContainsKey( key );
        }

        public bool TryGetValue(TKey key, out int value)
        {
            if ( _map.TryGetValue( key, out var @ref ) )
            {
                value = @ref.Value;
                return true;
            }

            value = default;
            return false;
        }

        public IEnumerator<KeyValuePair<TKey, int>> GetEnumerator()
        {
            return _map.Select( v => KeyValuePair.Create( v.Key, v.Value.Value ) ).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
