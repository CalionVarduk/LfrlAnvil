using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}

public readonly record struct SqlQueryReader<TRow>(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<TRow>> Delegate)
    where TRow : notnull
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryResult<TRow> Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}
