using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an asynchronous type-erased query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
public readonly record struct SqlAsyncQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult>> Delegate
)
{
    /// <summary>
    /// Asynchronously reads a collection of rows.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlQueryResult> ReadAsync(
        IDataReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Delegate( reader, options ?? default, cancellationToken );
    }
}

/// <summary>
/// Represents an asynchronous generic query reader.
/// </summary>
/// <param name="Dialect">SQL dialect with which this query reader is associated.</param>
/// <param name="Delegate">Underlying delegate.</param>
/// <typeparam name="TRow">Row type.</typeparam>
public readonly record struct SqlAsyncQueryReader<TRow>(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<TRow>>> Delegate
)
    where TRow : notnull
{
    /// <summary>
    /// Asynchronously reads a collection of rows.
    /// </summary>
    /// <param name="reader"><see cref="IDataReader"/> to read from.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlQueryResult<TRow>> ReadAsync(
        IDataReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Delegate( reader, options ?? default, cancellationToken );
    }
}
