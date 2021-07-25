using System.Collections;
using System.Collections.Generic;

namespace LfrlSoft.NET.Core.Collections.Internal
{
    internal sealed class RingEnumerator<T> : IEnumerator<T?>
    {
        private int _index;
        private int _stepsLeft;
        private readonly T?[] _items;

        internal RingEnumerator(T?[] items, int startIndex)
        {
            _items = items;
            _index = (startIndex == 0 ? _items.Length : startIndex) - 1;
            _stepsLeft = _items.Length;
        }

        public T? Current => _items[_index];
        object? IEnumerator.Current => Current;

        public void Dispose() { }

        public bool MoveNext()
        {
            if ( _stepsLeft <= 0 )
                return false;

            --_stepsLeft;

            if ( ++_index == _items.Length )
                _index = 0;

            return true;
        }

        void IEnumerator.Reset()
        {
            _index -= (_items.Length - _stepsLeft);

            if ( _index < 0 )
                _index += _items.Length;

            _stepsLeft = _items.Length;
        }
    }
}
