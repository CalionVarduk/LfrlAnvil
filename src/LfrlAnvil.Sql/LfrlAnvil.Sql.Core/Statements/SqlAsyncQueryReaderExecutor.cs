using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncQueryReaderExecutor(SqlAsyncQueryReader Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryResult> ExecuteAsync(
        IDbCommand command,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, options, cancellationToken ).ConfigureAwait( false );
    }
}

public readonly record struct SqlAsyncQueryReaderExecutor<TRow>(SqlAsyncQueryReader<TRow> Reader, string Sql)
    where TRow : notnull
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryResult<TRow>> ExecuteAsync(
        IDbCommand command,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, options, cancellationToken ).ConfigureAwait( false );
    }
}
