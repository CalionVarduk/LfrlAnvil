using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Collections;

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
        using var enumerator = new Enumerator( _items, GetWrappedIndex( readIndex ) );

        while ( enumerator.MoveNext() )
            yield return enumerator.Current;
    }

    [Pure]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _items, GetWrappedIndex( _writeIndex ) );
    }

    [Pure]
    IEnumerator<T?> IEnumerable<T?>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public struct Enumerator : IEnumerator<T?>
    {
        private int _index;
        private int _stepsLeft;
        private readonly T?[] _items;

        internal Enumerator(T?[] items, int startIndex)
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
            _index -= _items.Length - _stepsLeft;

            if ( _index < 0 )
                _index += _items.Length;

            _stepsLeft = _items.Length;
        }
    }
}
