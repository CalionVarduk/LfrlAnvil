using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult>> Delegate
)
{
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

public readonly record struct SqlAsyncQueryReader<TRow>(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, CancellationToken, ValueTask<SqlQueryResult<TRow>>> Delegate
)
    where TRow : notnull
{
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
