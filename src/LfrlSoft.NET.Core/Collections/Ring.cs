using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LfrlSoft.NET.Core.Collections.Internal;

namespace LfrlSoft.NET.Core.Collections
{
    public class Ring<T> : IReadOnlyList<T?>
    {
        private int _startIndex;
        private readonly T?[] _items;

        public Ring(int count)
        {
            Assert.IsGreaterThan( count, 0, nameof( count ) );
            _items = new T?[count];
            _startIndex = 0;
        }

        public Ring(IEnumerable<T?> range)
        {
            _items = range.ToArray();
            Assert.IsGreaterThan( _items.Length, 0, $"{nameof( range )}.{nameof( Enumerable.Count )}" );
            _startIndex = 0;
        }

        public Ring(params T?[] range)
            : this( range.AsEnumerable() ) { }

        public T? this[int index]
        {
            get => _items[GetUnderlyingIndex( index )];
            set => _items[GetUnderlyingIndex( index )] = value;
        }

        public int Count => _items.Length;

        public int StartIndex
        {
            get => _startIndex;
            set
            {
                _startIndex = value % _items.Length;
                if ( _startIndex < 0 )
                    _startIndex += _items.Length;
            }
        }

        public int GetUnderlyingIndex(int index)
        {
            var i = (index + _startIndex) % _items.Length;
            return i < 0 ? i + _items.Length : i;
        }

        public void SetNext(T item)
        {
            _items[_startIndex] = item;

            if ( ++_startIndex == _items.Length )
                _startIndex = 0;
        }

        public void Clear()
        {
            for ( var i = 0; i < Count; ++i )
                _items[i] = default;

            _startIndex = 0;
        }

        public IEnumerator<T?> GetEnumerator()
        {
            return new RingEnumerator<T>( _items, _startIndex );
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
