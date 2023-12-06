using System;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly record struct SqlQueryReader(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult> Delegate)
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}

public readonly record struct SqlQueryReader<TRow>(
    SqlDialect Dialect,
    Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<TRow>> Delegate)
    where TRow : notnull
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult<TRow> Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}
