using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlMultiDataReader : IDisposable
{
    public SqlMultiDataReader(IDataReader reader)
    {
        Reader = reader;
    }

    public IDataReader Reader { get; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( ! Reader.IsClosed )
            Reader.Dispose();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public List<SqlQueryReaderResult> ReadAll(SqlQueryReader reader)
    {
        var result = new List<SqlQueryReaderResult>();
        while ( ! Reader.IsClosed )
            result.Add( Read( reader ) );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult Read(SqlQueryReader reader, SqlQueryReaderOptions? options = null)
    {
        var result = reader.Read( Reader, options );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public SqlQueryReaderResult<TRow> Read<TRow>(SqlQueryReader<TRow> reader, SqlQueryReaderOptions? options = null)
        where TRow : notnull
    {
        var result = reader.Read( Reader, options );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public TResult Read<TResult>(Func<IDataReader, TResult> reader)
    {
        var result = reader( Reader );
        if ( ! Reader.NextResult() )
            Reader.Dispose();

        return result;
    }
}
