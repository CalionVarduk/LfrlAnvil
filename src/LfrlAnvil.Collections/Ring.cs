using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Collections.Internal;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections
{
    public class Ring<T> : IRing<T>
    {
        private int _writeIndex;
        private readonly T?[] _items;

        public Ring(int count)
        {
            Ensure.IsGreaterThan( count, 0, nameof( count ) );
            _items = new T?[count];
            _writeIndex = 0;
        }

        public Ring(IEnumerable<T?> range)
        {
            _items = range.ToArray();
            Ensure.IsGreaterThan( _items.Length, 0, $"{nameof( range )}.{nameof( Enumerable.Count )}" );
            _writeIndex = 0;
        }

        public Ring(params T?[] range)
            : this( range.AsEnumerable() ) { }

        public T? this[int index]
        {
            get => _items[index];
            set => _items[index] = value;
        }

        public int Count => _items.Length;

        public int WriteIndex
        {
            get => _writeIndex;
            set => _writeIndex = GetWrappedIndex( value );
        }

        [Pure]
        public int GetWrappedIndex(int index)
        {
            return index.EuclidModulo( _items.Length );
        }

        [Pure]
        public int GetWriteIndex(int offset)
        {
            return GetWrappedIndex( _writeIndex + offset );
        }

        public void SetNext(T item)
        {
            _items[_writeIndex] = item;

            if ( ++_writeIndex == _items.Length )
                _writeIndex = 0;
        }

        public void Clear()
        {
            for ( var i = 0; i < _items.Length; ++i )
                _items[i] = default;

            _writeIndex = 0;
        }

        [Pure]
        public IEnumerable<T?> Read(int readIndex)
        {
            using var enumerator = new RingEnumerator<T>( _items, GetWrappedIndex( readIndex ) );

            while ( enumerator.MoveNext() )
                yield return enumerator.Current;
        }

        [Pure]
        public IEnumerator<T?> GetEnumerator()
        {
            return new RingEnumerator<T>( _items, GetWrappedIndex( _writeIndex + 1 ) );
        }

        [Pure]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
