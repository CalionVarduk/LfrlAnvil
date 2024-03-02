using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncScalarReaderExecutor(SqlAsyncScalarReader Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlScalarResult> ExecuteAsync(IDbCommand command, CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, cancellationToken ).ConfigureAwait( false );
    }
}

public readonly record struct SqlAsyncScalarReaderExecutor<T>(SqlAsyncScalarReader<T> Reader, string Sql)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlScalarResult<T>> ExecuteAsync(IDbCommand command, CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        await using var reader = await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await Reader.ReadAsync( reader, cancellationToken ).ConfigureAwait( false );
    }
}
