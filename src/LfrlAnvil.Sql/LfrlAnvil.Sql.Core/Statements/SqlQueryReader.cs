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

public readonly struct SqlQueryReader<TRow>
    where TRow : notnull
{
    public SqlQueryReader(SqlDialect dialect, Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<TRow>> @delegate)
    {
        Dialect = dialect;
        Delegate = @delegate;
    }

    public SqlDialect Dialect { get; }
    public Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<TRow>> Delegate { get; }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult<TRow> Read(IDataReader reader, SqlQueryReaderOptions? options = null)
    {
        return Delegate( reader, options ?? default );
    }
}
