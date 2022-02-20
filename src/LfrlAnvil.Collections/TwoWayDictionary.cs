using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;

namespace LfrlAnvil.Collections
{
    public class TwoWayDictionary<T1, T2> : ITwoWayDictionary<T1, T2>
        where T1 : notnull
        where T2 : notnull
    {
        private readonly Dictionary<T1, T2> _forward;
        private readonly Dictionary<T2, T1> _reverse;

        public TwoWayDictionary()
        {
            _forward = new Dictionary<T1, T2>();
            _reverse = new Dictionary<T2, T1>();
        }

        public TwoWayDictionary(IEqualityComparer<T1> forwardComparer, IEqualityComparer<T2> reverseComparer)
        {
            _forward = new Dictionary<T1, T2>( forwardComparer );
            _reverse = new Dictionary<T2, T1>( reverseComparer );
        }

        public int Count => _forward.Count;
        public IReadOnlyDictionary<T1, T2> Forward => _forward;
        public IReadOnlyDictionary<T2, T1> Reverse => _reverse;
        public IEqualityComparer<T1> ForwardComparer => _forward.Comparer;
        public IEqualityComparer<T2> ReverseComparer => _reverse.Comparer;

        public bool TryAdd(T1 first, T2 second)
        {
            if ( _forward.ContainsKey( first ) || _reverse.ContainsKey( second ) )
                return false;

            _forward.Add( first, second );
            _reverse.Add( second, first );
            return true;
        }

        public void Add(T1 first, T2 second)
        {
            Ensure.False( _forward.ContainsKey( first ), "key already exists in forward dictionary" );
            Ensure.False( _reverse.ContainsKey( second ), "key already exists in reverse dictionary" );
            _forward.Add( first, second );
            _reverse.Add( second, first );
        }

        public bool TryUpdateForward(T1 first, T2 second)
        {
            if ( _reverse.ContainsKey( second ) )
                return false;

            if ( ! _forward.TryGetValue( first, out var other ) )
                return false;

            _forward[first] = second;
            _reverse.Remove( other );
            _reverse.Add( second, first );
            return true;
        }

        public void UpdateForward(T1 first, T2 second)
        {
            Ensure.False( _reverse.ContainsKey( second ), "key already exists in reverse dictionary" );
            var other = _forward[first];
            _forward[first] = second;
            _reverse.Remove( other );
            _reverse.Add( second, first );
        }

        public bool TryUpdateReverse(T2 second, T1 first)
        {
            if ( _forward.ContainsKey( first ) )
                return false;

            if ( ! _reverse.TryGetValue( second, out var other ) )
                return false;

            _reverse[second] = first;
            _forward.Remove( other );
            _forward.Add( first, second );
            return true;
        }

        public void UpdateReverse(T2 second, T1 first)
        {
            Ensure.False( _forward.ContainsKey( first ), "key already exists in forward dictionary" );
            var other = _reverse[second];
            _reverse[second] = first;
            _forward.Remove( other );
            _forward.Add( first, second );
        }

        public bool RemoveForward(T1 value)
        {
            return RemoveForward( value, out _ );
        }

        public bool RemoveReverse(T2 value)
        {
            return RemoveReverse( value, out _ );
        }

        public bool RemoveForward(T1 value, [MaybeNullWhen( false )] out T2 second)
        {
            if ( ! _forward.TryGetValue( value, out second ) )
                return false;

            _forward.Remove( value );
            _reverse.Remove( second );
            return true;
        }

        public bool RemoveReverse(T2 value, [MaybeNullWhen( false )] out T1 first)
        {
            if ( ! _reverse.TryGetValue( value, out first ) )
                return false;

            _reverse.Remove( value );
            _forward.Remove( first );
            return true;
        }

        public void Clear()
        {
            _forward.Clear();
            _reverse.Clear();
        }

        [Pure]
        public IEnumerator<Pair<T1, T2>> GetEnumerator()
        {
            return _forward.Select( kv => Pair.Create( kv.Key, kv.Value ) ).GetEnumerator();
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
