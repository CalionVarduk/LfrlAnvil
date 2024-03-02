using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlAsyncScalarReader(
    SqlDialect Dialect,
    Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlScalarResult> ReadAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        return Delegate( reader, cancellationToken );
    }
}

public readonly record struct SqlAsyncScalarReader<T>(
    SqlDialect Dialect,
    Func<IDataReader, CancellationToken, ValueTask<SqlScalarResult<T>>> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public ValueTask<SqlScalarResult<T>> ReadAsync(IDataReader reader, CancellationToken cancellationToken = default)
    {
        return Delegate( reader, cancellationToken );
    }
}
