using System;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Reactive.Internal
{
    internal sealed class GrowingBuffer<T>
    {
        internal const int BaseCapacity = 15;

        private T[] _data;
        private int _count;

        internal GrowingBuffer()
        {
            _data = new T[BaseCapacity];
            _count = 0;
        }

        public void Add(T item)
        {
            if ( _count == _data.Length )
            {
                var newData = new T[_data.Length * 2 + 1];
                for ( var i = 0; i < _count; ++i )
                    newData[i] = _data[i];

                _data = newData;
            }

            _data[_count++] = item;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void RemoveAll()
        {
            Array.Clear( _data, 0, _count );
            _count = 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public void Clear()
        {
            _data = Array.Empty<T>();
            _count = 0;
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public ReadOnlyMemory<T> AsMemory()
        {
            return _data.AsMemory( 0, _count );
        }
    }
}
