using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncScalarQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlScalarQueryResult> ReadAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        return Delegate( reader, cancellationToken );
    }
}

public readonly record struct SqlAsyncScalarQueryReader<T>(
    SqlDialect Dialect,
    Func<IDataReader, CancellationToken, ValueTask<SqlScalarQueryResult<T>>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlScalarQueryResult<T>> ReadAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        return Delegate( reader, cancellationToken );
    }
}
