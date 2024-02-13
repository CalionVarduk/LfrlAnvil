using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlIndexColumnBuilderArray<T> : IReadOnlyList<SqlIndexColumnBuilder<T>>
    where T : class, ISqlColumnBuilder
{
    private readonly ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> _source;

    private SqlIndexColumnBuilderArray(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> source)
    {
        _source = source;
    }

    public int Count => _source.Count;
    public SqlIndexColumnBuilder<T> this[int index] => _source[index].UnsafeReinterpretAs<T>();

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlIndexColumnBuilderArray<T> From(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> source)
    {
        return new SqlIndexColumnBuilderArray<T>( source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlIndexColumnBuilderArray<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlColumnBuilder
    {
        return new SqlIndexColumnBuilderArray<TDestination>( _source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    public struct Enumerator : IEnumerator<SqlIndexColumnBuilder<T>>
    {
        private ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>>.Enumerator _source;

        internal Enumerator(ReadOnlyArray<SqlIndexColumnBuilder<ISqlColumnBuilder>> source)
        {
            _source = source.GetEnumerator();
        }

        public SqlIndexColumnBuilder<T> Current => _source.Current.UnsafeReinterpretAs<T>();
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
    IEnumerator<SqlIndexColumnBuilder<T>> IEnumerable<SqlIndexColumnBuilder<T>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    [Pure]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
