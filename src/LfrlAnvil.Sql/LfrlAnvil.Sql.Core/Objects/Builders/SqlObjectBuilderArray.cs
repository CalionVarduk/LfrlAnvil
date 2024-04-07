using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Objects.Builders;

public readonly struct SqlObjectBuilderArray<T> : IReadOnlyList<T>
    where T : class, ISqlObjectBuilder
{
    private readonly ReadOnlyArray<SqlObjectBuilder> _source;

    private SqlObjectBuilderArray(ReadOnlyArray<SqlObjectBuilder> source)
    {
        _source = source;
    }

    public int Count => _source.Count;
    public T this[int index] => ReinterpretCast.To<T>( _source[index] );

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlObjectBuilderArray<T> From<TSource>(ReadOnlyArray<TSource> source)
        where TSource : SqlObjectBuilder
    {
        return new SqlObjectBuilderArray<T>( ReadOnlyArray<SqlObjectBuilder>.From( source ) );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderArray<TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : class, ISqlObjectBuilder
    {
        return new SqlObjectBuilderArray<TDestination>( _source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public Enumerator GetEnumerator()
    {
        return new Enumerator( _source );
    }

    public struct Enumerator : IEnumerator<T>
    {
        private ReadOnlyArray<SqlObjectBuilder>.Enumerator _source;

        internal Enumerator(ReadOnlyArray<SqlObjectBuilder> source)
        {
            _source = source.GetEnumerator();
        }

        public T Current => ReinterpretCast.To<T>( _source.Current );
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
