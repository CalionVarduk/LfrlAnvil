using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryReaderResult>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlQueryReaderResult> ReadAsync(
        IDataReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Delegate( reader, options ?? default, cancellationToken );
    }
}

public readonly record struct SqlAsyncQueryReader<TRow>(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryReaderResult<TRow>>> Delegate)
    where TRow : notnull
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlQueryReaderResult<TRow>> ReadAsync(
        IDataReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return Delegate( reader, options ?? default, cancellationToken );
    }
}
