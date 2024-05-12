using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an asynchronous type-erased scalar query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
public readonly record struct SqlAsyncScalarQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult>> Delegate
)
{
    /// <summary>
    /// Asynchronously reads a scalar value.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlScalarQueryResult> ReadAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        return Delegate( reader, cancellationToken );
    }
}

/// <summary>
/// Represents an asynchronous generic scalar query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
/// <typeparam name="T">Value type.</typeparam>
public readonly record struct SqlAsyncScalarQueryReader<T>(
    SqlDialect Dialect,
    Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<T>>> Delegate
)
{
    /// <summary>
    /// Asynchronously reads a scalar value.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlScalarQueryResult<T>> ReadAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        return Delegate( reader, cancellationToken );
    }
}
