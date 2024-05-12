using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents a lightweight enumerator of a collection of <see cref="SqlObjectBuilder"/> instances.
/// </summary>
/// <typeparam name="T">SQL object builder type.</typeparam>
public struct SqlObjectBuilderEnumerator<T> : IEnumerator<T>
    where T : SqlObjectBuilder
{
    private Dictionary<string, T>.ValueCollection.Enumerator _enumerator;

    internal SqlObjectBuilderEnumerator(Dictionary<string, T> source)
    {
        _enumerator = source.Values.GetEnumerator();
    }

    /// <inheritdoc />
    public T Current => _enumerator.Current;

    object IEnumerator.Current => Current;

    /// <summary>
    /// Creates a new <see cref="SqlObjectBuilderEnumerator{T,TDestination}"/> instance.
    /// </summary>
    /// <typeparam name="TDestination">Destination SQL object builder type.</typeparam>
    /// <returns>New <see cref="SqlObjectBuilderEnumerator{T,TDestination}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlObjectBuilderEnumerator<T, TDestination> UnsafeReinterpretAs<TDestination>()
        where TDestination : T
    {
        return new SqlObjectBuilderEnumerator<T, TDestination>( _enumerator );
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _enumerator.Dispose();
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    void IEnumerator.Reset()
    {
        (( IEnumerator )_enumerator).Reset();
    }
}

/// <summary>
/// Represents a lightweight enumerator of a collection of <see cref="SqlObjectBuilder"/> instances.
/// </summary>
/// <typeparam name="TSource">Source SQL object builder type.</typeparam>
/// <typeparam name="TDestination">Destination SQL object builder type.</typeparam>
public struct SqlObjectBuilderEnumerator<TSource, TDestination> : IEnumerator<TDestination>
    where TSource : SqlObjectBuilder
    where TDestination : TSource
{
    private Dictionary<string, TSource>.ValueCollection.Enumerator _enumerator;

    internal SqlObjectBuilderEnumerator(Dictionary<string, TSource>.ValueCollection.Enumerator enumerator)
    {
        _enumerator = enumerator;
    }

    /// <inheritdoc />
    public TDestination Current => ReinterpretCast.To<TDestination>( _enumerator.Current );

    object IEnumerator.Current => Current;

    /// <inheritdoc />
    public void Dispose()
    {
        _enumerator.Dispose();
    }

    /// <inheritdoc />
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public bool MoveNext()
    {
        return _enumerator.MoveNext();
    }

    void IEnumerator.Reset()
    {
        (( IEnumerator )_enumerator).Reset();
    }
}
