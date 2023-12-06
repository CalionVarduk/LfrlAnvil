using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public readonly struct SqlAsyncMultiDataReader : IDisposable, IAsyncDisposable
{
    public SqlAsyncMultiDataReader(IDataReader reader)
    {
        Reader = (DbDataReader)reader;
    }

    public DbDataReader Reader { get; }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public void Dispose()
    {
        if ( ! Reader.IsClosed )
            Reader.Dispose();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask DisposeAsync()
    {
        if ( ! Reader.IsClosed )
            await Reader.DisposeAsync().ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<List<SqlQueryReaderResult>> ReadAllAsync(
        SqlAsyncQueryReader reader,
        CancellationToken cancellationToken = default)
    {
        var result = new List<SqlQueryReaderResult>();
        while ( ! Reader.IsClosed )
            result.Add( await ReadAsync( reader, null, cancellationToken ).ConfigureAwait( false ) );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryReaderResult> ReadAsync(
        SqlAsyncQueryReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        var result = await reader.ReadAsync( Reader, options, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<SqlQueryReaderResult<TRow>> ReadAsync<TRow>(
        SqlAsyncQueryReader<TRow> reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRow : notnull
    {
        var result = await reader.ReadAsync( Reader, options, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<TResult> ReadAsync<TResult>(
        Func<DbDataReader, CancellationToken, ValueTask<TResult>> reader,
        CancellationToken cancellationToken = default)
    {
        var result = await reader( Reader, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public async ValueTask<TResult> ReadAsync<TResult>(
        Func<DbDataReader, CancellationToken, Task<TResult>> reader,
        CancellationToken cancellationToken = default)
    {
        var result = await reader( Reader, cancellationToken ).ConfigureAwait( false );
        if ( ! await Reader.NextResultAsync( cancellationToken ).ConfigureAwait( false ) )
            await Reader.DisposeAsync().ConfigureAwait( false );

        return result;
    }
}
