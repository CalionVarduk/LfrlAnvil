using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

public readonly struct SqlIndexedArray<T> : IReadOnlyList<SqlIndexed<T>>
    where T : class, ISqlColumn
{
    private readonly ReadOnlyArray<SqlIndexed<ISqlColumn>> _source;

    private SqlIndexedArray(ReadOnlyArray<SqlIndexed<ISqlColumn>> source)
    {
        _source = source;
    }

    public int Count => _source.Count;

    public SqlIndexed<T> this[int index]
    {
        get
        {
            var source = _source[index];
            return new SqlIndexed<T>( ReinterpretCast.To<T>( source.Column ), source.Ordering );
        }
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexedArray<T> From(ReadOnlyArray<SqlIndexed<ISqlColumn>> source)
    {
        return new SqlIndexedArray<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlIndexedArray<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlColumn
    {
        return new SqlIndexedArray<TDestination>( _source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    public struct Enumerator : IEnumerator<SqlIndexed<T>>
    {
        private ReadOnlyArray<SqlIndexed<ISqlColumn>>.Enumerator _source;

        internal Enumerator(ReadOnlyArray<SqlIndexed<ISqlColumn>> source)
        {
            _source = source.GetEnumerator();
        }

        public SqlIndexed<T> Current
        {
            get
            {
                var source = _source.Current;
                return new SqlIndexed<T>( ReinterpretCast.To<T>( source.Column ), source.Ordering );
            }
        }

        object IEnumerator.Current => Current;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        public bool MoveNext()
        {
            return _source.MoveNext();
        }

        public void Dispose()
        {
            _source.Dispose();
        }

        void IEnumerator.Reset()
        {
            (( IEnumerator )_source).Reset();
        }
    }

    [Pure]
    IEnumerator<SqlIndexed<T>> IEnumerable<SqlIndexed<T>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
