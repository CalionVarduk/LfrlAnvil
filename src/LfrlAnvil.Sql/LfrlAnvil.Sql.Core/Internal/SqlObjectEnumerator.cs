using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.Sql.Internal;

public struct SqlObjectEnumerator<T> : IEnumerator<T>
    where T : SqlObject
{
    private Dictionary<string, T>.ValueCollection.Enumerator _enumerator;

    internal SqlObjectEnumerator(Dictionary<string, T> source)
    {
        _enumerator = source.Values.GetEnumerator();
    }

    public T Current => _enumerator.Current;
    object IEnumerator.Current => Current;

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectEnumerator<T, TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : T
    {
        return new SqlObjectEnumerator<T, TDestination>( _enumerator );
    }

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    void IEnumerator.Reset()
    {
        ((IEnumerator)_enumerator).Reset();
    }
}

public struct SqlObjectEnumerator<TSource, TDestination> : IEnumerator<TDestination>
    where TSource : SqlObject
    where TDestination : TSource
{
    private Dictionary<string, TSource>.ValueCollection.Enumerator _enumerator;

    internal SqlObjectEnumerator(Dictionary<string, TSource>.ValueCollection.Enumerator enumerator)
    {
        _enumerator = enumerator;
    }

    public TDestination Current => ReinterpretCast.To<TDestination>( _enumerator.Current );
    object IEnumerator.Current => Current;

    public void Dispose()
    {
        _enumerator.Dispose();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    void IEnumerator.Reset()
    {
        ((IEnumerator)_enumerator).Reset();
    }
}
