using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

public static class SqlStatementObjectExtensions
{
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<DbTransaction> BeginTransactionAsync(
        this IDbConnection connection,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        return await ((DbConnection)connection).BeginTransactionAsync( isolationLevel, cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataReader Multi(this IDataReader reader)
    {
        return new SqlMultiDataReader( reader );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataReader MultiQuery(this IDbCommand command)
    {
        return command.ExecuteReader().Multi();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderResult Query(this IDbCommand command, SqlQueryReader reader, SqlQueryReaderOptions? options = null)
    {
        using var r = command.ExecuteReader();
        return reader.Read( r, options );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderResult<TRow> Query<TRow>(
        this IDbCommand command,
        SqlQueryReader<TRow> reader,
        SqlQueryReaderOptions? options = null)
        where TRow : notnull
    {
        using var r = command.ExecuteReader();
        return reader.Read( r, options );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderResult Query(
        this IDbCommand command,
        SqlQueryReaderExecutor executor,
        SqlQueryReaderOptions? options = null)
    {
        return executor.Execute( command, options );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderResult<TRow> Query<TRow>(
        this IDbCommand command,
        SqlQueryReaderExecutor<TRow> executor,
        SqlQueryReaderOptions? options = null)
        where TRow : notnull
    {
        return executor.Execute( command, options );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarResult Query(this IDbCommand command, SqlScalarReader reader)
    {
        using var r = command.ExecuteReader();
        return reader.Read( r );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarResult<T> Query<T>(this IDbCommand command, SqlScalarReader<T> reader)
    {
        using var r = command.ExecuteReader();
        return reader.Read( r );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarResult Query(this IDbCommand command, SqlScalarReaderExecutor executor)
    {
        return executor.Execute( command );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarResult<T> Query<T>(this IDbCommand command, SqlScalarReaderExecutor<T> executor)
    {
        return executor.Execute( command );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int Execute(this IDbCommand command)
    {
        return command.ExecuteNonQuery();
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand SetText<TCommand>(this TCommand command, string sql)
        where TCommand : IDbCommand
    {
        command.CommandText = sql;
        return command;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand SetTimeout<TCommand>(this TCommand command, TimeSpan timeout)
        where TCommand : IDbCommand
    {
        command.CommandTimeout = (int)Math.Ceiling( timeout.TotalSeconds );
        return command;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand Parameterize<TCommand>(this TCommand command, SqlParameterBinderExecutor executor)
        where TCommand : IDbCommand
    {
        executor.Execute( command );
        return command;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand Parameterize<TCommand, TSource>(this TCommand command, SqlParameterBinderExecutor<TSource> executor)
        where TCommand : IDbCommand
        where TSource : notnull
    {
        executor.Execute( command );
        return command;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderExecutor Bind(this SqlQueryReader reader, string sql)
    {
        return new SqlQueryReaderExecutor( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderExecutor<TRow> Bind<TRow>(this SqlQueryReader<TRow> reader, string sql)
        where TRow : notnull
    {
        return new SqlQueryReaderExecutor<TRow>( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarReaderExecutor Bind(this SqlScalarReader reader, string sql)
    {
        return new SqlScalarReaderExecutor( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarReaderExecutor<T> Bind<T>(this SqlScalarReader<T> reader, string sql)
    {
        return new SqlScalarReaderExecutor<T>( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterBinderExecutor Bind(this SqlParameterBinder binder, IEnumerable<KeyValuePair<string, object?>>? source)
    {
        return new SqlParameterBinderExecutor( binder, source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterBinderExecutor<TSource> Bind<TSource>(this SqlParameterBinder<TSource> binder, TSource? source)
        where TSource : notnull
    {
        return new SqlParameterBinderExecutor<TSource>( binder, source );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncMultiDataReader MultiAsync(this IDataReader reader)
    {
        return new SqlAsyncMultiDataReader( reader );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlAsyncMultiDataReader> MultiQueryAsync(
        this IDbCommand command,
        CancellationToken cancellationToken = default)
    {
        return (await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false )).MultiAsync();
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlQueryReaderResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncQueryReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var r = await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, options, cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlQueryReaderResult<TRow>> QueryAsync<TRow>(
        this IDbCommand command,
        SqlAsyncQueryReader<TRow> reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRow : notnull
    {
        await using var r = await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, options, cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlQueryReaderResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncQueryReaderExecutor executor,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync( command, options, cancellationToken );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlQueryReaderResult<TRow>> QueryAsync<TRow>(
        this IDbCommand command,
        SqlAsyncQueryReaderExecutor<TRow> executor,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRow : notnull
    {
        return executor.ExecuteAsync( command, options, cancellationToken );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlScalarResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncScalarReader reader,
        CancellationToken cancellationToken = default)
    {
        await using var r = await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlScalarResult<T>> QueryAsync<T>(
        this IDbCommand command,
        SqlAsyncScalarReader<T> reader,
        CancellationToken cancellationToken = default)
    {
        await using var r = await ((DbCommand)command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlScalarResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncScalarReaderExecutor executor,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync( command, cancellationToken );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlScalarResult<T>> QueryAsync<T>(
        this IDbCommand command,
        SqlAsyncScalarReaderExecutor<T> executor,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync( command, cancellationToken );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<int> ExecuteAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        return await ((DbCommand)command).ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryReaderExecutor Bind(this SqlAsyncQueryReader reader, string sql)
    {
        return new SqlAsyncQueryReaderExecutor( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryReaderExecutor<TRow> Bind<TRow>(this SqlAsyncQueryReader<TRow> reader, string sql)
        where TRow : notnull
    {
        return new SqlAsyncQueryReaderExecutor<TRow>( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarReaderExecutor Bind(this SqlAsyncScalarReader reader, string sql)
    {
        return new SqlAsyncScalarReaderExecutor( reader, sql );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarReaderExecutor<T> Bind<T>(this SqlAsyncScalarReader<T> reader, string sql)
    {
        return new SqlAsyncScalarReaderExecutor<T>( reader, sql );
    }
}
