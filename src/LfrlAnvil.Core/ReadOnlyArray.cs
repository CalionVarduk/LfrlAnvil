using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil;

public readonly struct ReadOnlyArray<T> : IReadOnlyList<T>
{
    public static readonly ReadOnlyArray<T> Empty = new ReadOnlyArray<T>( Array.Empty<T>() );

    private readonly T[] _source;

    public ReadOnlyArray(T[] source)
    {
        _source = source;
    }

    public int Count => _source.Length;
    public T this[int index] => _source[index];

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlyArray<T> From<TSource>(TSource[] source)
        where TSource : class?, T
    {
        return new ReadOnlyArray<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ReadOnlyArray<T> From<TSource>(ReadOnlyArray<TSource> source)
        where TSource : class?, T
    {
        return From( source._source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlyMemory<T> AsMemory()
    {
        return _source;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ReadOnlySpan<T> AsSpan()
    {
        return _source;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public IReadOnlyList<T> GetUnderlyingArray()
    {
        return _source;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static implicit operator ReadOnlyArray<T>(T[] source)
    {
        return new ReadOnlyArray<T>( source );
    }

    public struct Enumerator : IEnumerator<T>
    {
        private readonly T[] _source;
        private int _index;

        internal Enumerator(T[] source)
        {
            _source = source;
            _index = -1;
        }

        public T Current => _source[_index];
        object? IEnumerator.Current => Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            var i = _index + 1;
            if ( i >= _source.Length )
                return false;

            _index = i;
            return true;
        }

        public void Dispose() { }

        void IEnumerator.Reset()
        {
            _index = -1;
        }
    }

    [Pure]
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
