using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects;

public readonly struct SqlIndexColumnArray<T> : IReadOnlyList<SqlIndexColumn<T>>
    where T : class, ISqlColumn
{
    private readonly ReadOnlyArray<SqlIndexColumn<ISqlColumn>> _source;

    private SqlIndexColumnArray(ReadOnlyArray<SqlIndexColumn<ISqlColumn>> source)
    {
        _source = source;
    }

    public int Count => _source.Count;
    public SqlIndexColumn<T> this[int index] => _source[index].UnsafeReinterpretAs<T>();

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumnArray<T> From(ReadOnlyArray<SqlIndexColumn<ISqlColumn>> source)
    {
        return new SqlIndexColumnArray<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlIndexColumnArray<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlColumn
    {
        return new SqlIndexColumnArray<TDestination>( _source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    public struct Enumerator : IEnumerator<SqlIndexColumn<T>>
    {
        private ReadOnlyArray<SqlIndexColumn<ISqlColumn>>.Enumerator _source;

        internal Enumerator(ReadOnlyArray<SqlIndexColumn<ISqlColumn>> source)
        {
            _source = source.GetEnumerator();
        }

        public SqlIndexColumn<T> Current => _source.Current.UnsafeReinterpretAs<T>();
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
            ((IEnumerator)_source).Reset();
        }
    }

    [Pure]
    IEnumerator<SqlIndexColumn<T>> IEnumerable<SqlIndexColumn<T>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
