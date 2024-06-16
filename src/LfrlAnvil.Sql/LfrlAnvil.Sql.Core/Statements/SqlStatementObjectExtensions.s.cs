// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Contains various extension methods related to SQL statements.
/// </summary>
public static class SqlStatementObjectExtensions
{
    /// <summary>
    /// Asynchronously creates a new <see cref="DbTransaction"/> instance from the provided <paramref name="connection"/>.
    /// </summary>
    /// <param name="connection">Source connection.</param>
    /// <param name="isolationLevel">Transaction's <see cref="IsolationLevel"/>.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{Tresult}"/> that returns a new <see cref="DbTransaction"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<DbTransaction> BeginTransactionAsync(
        this IDbConnection connection,
        IsolationLevel isolationLevel,
        CancellationToken cancellationToken = default)
    {
        return await (( DbConnection )connection).BeginTransactionAsync( isolationLevel, cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// Creates a new <see cref="IDbCommand"/> instance associated with the provided <paramref name="transaction"/>.
    /// </summary>
    /// <param name="transaction">Source transaction.</param>
    /// <returns>New <see cref="IDbCommand"/> instance.</returns>
    public static IDbCommand CreateCommand(this IDbTransaction transaction)
    {
        Ensure.IsNotNull( transaction.Connection );
        var result = transaction.Connection.CreateCommand();
        result.Transaction = transaction;
        return result;
    }

    /// <summary>
    /// Creates a new <see cref="DbCommand"/> instance associated with the provided <paramref name="transaction"/>.
    /// </summary>
    /// <param name="transaction">Source transaction.</param>
    /// <returns>New <see cref="DbCommand"/> instance.</returns>
    public static DbCommand CreateCommand(this DbTransaction transaction)
    {
        Ensure.IsNotNull( transaction.Connection );
        var result = transaction.Connection.CreateCommand();
        result.Transaction = transaction;
        return result;
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataReader"/> instance.
    /// </summary>
    /// <param name="reader">Source data reader.</param>
    /// <returns>New <see cref="SqlMultiDataReader"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataReader Multi(this IDataReader reader)
    {
        return new SqlMultiDataReader( reader );
    }

    /// <summary>
    /// Creates a new <see cref="SqlMultiDataReader"/> instance.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <returns>New <see cref="SqlMultiDataReader"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlMultiDataReader MultiQuery(this IDbCommand command)
    {
        return command.ExecuteReader().Multi();
    }

    /// <summary>
    /// Reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlQueryReader"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryResult Query(this IDbCommand command, SqlQueryReader reader, SqlQueryReaderOptions? options = null)
    {
        using var r = command.ExecuteReader();
        return reader.Read( r, options );
    }

    /// <summary>
    /// Reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlQueryReader{TRow}"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryResult<TRow> Query<TRow>(
        this IDbCommand command,
        SqlQueryReader<TRow> reader,
        SqlQueryReaderOptions? options = null)
        where TRow : notnull
    {
        using var r = command.ExecuteReader();
        return reader.Read( r, options );
    }

    /// <summary>
    /// Reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlQueryReaderExecutor"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryResult Query(this IDbCommand command, SqlQueryReaderExecutor executor, SqlQueryReaderOptions? options = null)
    {
        return executor.Execute( command, options );
    }

    /// <summary>
    /// Reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlQueryReaderExecutor{TRow}"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>Returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryResult<TRow> Query<TRow>(
        this IDbCommand command,
        SqlQueryReaderExecutor<TRow> executor,
        SqlQueryReaderOptions? options = null)
        where TRow : notnull
    {
        return executor.Execute( command, options );
    }

    /// <summary>
    /// Reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlScalarQueryReader"/> to use for reading.</param>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryResult Query(this IDbCommand command, SqlScalarQueryReader reader)
    {
        using var r = command.ExecuteReader();
        return reader.Read( r );
    }

    /// <summary>
    /// Reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlScalarQueryReader{T}"/> to use for reading.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryResult<T> Query<T>(this IDbCommand command, SqlScalarQueryReader<T> reader)
    {
        using var r = command.ExecuteReader();
        return reader.Read( r );
    }

    /// <summary>
    /// Reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlScalarQueryReaderExecutor"/> to use for reading.</param>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryResult Query(this IDbCommand command, SqlScalarQueryReaderExecutor executor)
    {
        return executor.Execute( command );
    }

    /// <summary>
    /// Reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlScalarQueryReaderExecutor{T}"/> to use for reading.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryResult<T> Query<T>(this IDbCommand command, SqlScalarQueryReaderExecutor<T> executor)
    {
        return executor.Execute( command );
    }

    /// <summary>
    /// Executes the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <returns>The number of rows affected.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static int Execute(this IDbCommand command)
    {
        return command.ExecuteNonQuery();
    }

    /// <summary>
    /// Updates the <see cref="IDbCommand.CommandText"/> of the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="sql">Value to set.</param>
    /// <typeparam name="TCommand">DB command type.</typeparam>
    /// <returns><paramref name="command"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand SetText<TCommand>(this TCommand command, string sql)
        where TCommand : IDbCommand
    {
        command.CommandText = sql;
        return command;
    }

    /// <summary>
    /// Updates the <see cref="IDbCommand.CommandTimeout"/> of the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="timeout">Value to set.</param>
    /// <typeparam name="TCommand">DB command type.</typeparam>
    /// <returns><paramref name="command"/>.</returns>
    /// <exception cref="ArgumentException">When <paramref name="timeout"/> is less <b>0</b>.</exception>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand SetTimeout<TCommand>(this TCommand command, TimeSpan timeout)
        where TCommand : IDbCommand
    {
        command.CommandTimeout = ( int )Math.Ceiling( timeout.TotalSeconds );
        return command;
    }

    /// <summary>
    /// Binds parameters to the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlParameterBinderExecutor"/> to use for binding.</param>
    /// <typeparam name="TCommand">DB command type.</typeparam>
    /// <returns><paramref name="command"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand Parameterize<TCommand>(this TCommand command, SqlParameterBinderExecutor executor)
        where TCommand : IDbCommand
    {
        executor.Execute( command );
        return command;
    }

    /// <summary>
    /// Binds parameters to the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlParameterBinderExecutor{TSource}"/> to use for binding.</param>
    /// <typeparam name="TCommand">DB command type.</typeparam>
    /// <typeparam name="TSource">Parameter source type.</typeparam>
    /// <returns><paramref name="command"/>.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static TCommand Parameterize<TCommand, TSource>(this TCommand command, SqlParameterBinderExecutor<TSource> executor)
        where TCommand : IDbCommand
        where TSource : notnull
    {
        executor.Execute( command );
        return command;
    }

    /// <summary>
    /// Binds the provided <see cref="SqlQueryReader"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <returns>New <see cref="SqlQueryReaderExecutor"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderExecutor Bind(this SqlQueryReader reader, string sql)
    {
        return new SqlQueryReaderExecutor( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlQueryReader{TRow}"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="SqlQueryReaderExecutor{TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlQueryReaderExecutor<TRow> Bind<TRow>(this SqlQueryReader<TRow> reader, string sql)
        where TRow : notnull
    {
        return new SqlQueryReaderExecutor<TRow>( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlScalarQueryReader"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source scalar query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <returns>New <see cref="SqlScalarQueryReaderExecutor"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryReaderExecutor Bind(this SqlScalarQueryReader reader, string sql)
    {
        return new SqlScalarQueryReaderExecutor( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlScalarQueryReader{T}"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source scalar query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlScalarQueryReaderExecutor{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlScalarQueryReaderExecutor<T> Bind<T>(this SqlScalarQueryReader<T> reader, string sql)
    {
        return new SqlScalarQueryReaderExecutor<T>( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlParameterBinder"/> to the given parameter <paramref name="source"/>.
    /// </summary>
    /// <param name="binder">Source parameter binder.</param>
    /// <param name="source">Parameter source to bind with.</param>
    /// <returns>New <see cref="SqlParameterBinderExecutor"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterBinderExecutor Bind(this SqlParameterBinder binder, IEnumerable<SqlParameter>? source)
    {
        return new SqlParameterBinderExecutor( binder, source );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlParameterBinder{TSource}"/> to the given parameter <paramref name="source"/>.
    /// </summary>
    /// <param name="binder">Source parameter binder.</param>
    /// <param name="source">Parameter source to bind with.</param>
    /// <typeparam name="TSource">Parameter source type.</typeparam>
    /// <returns>New <see cref="SqlParameterBinderExecutor{TSource}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlParameterBinderExecutor<TSource> Bind<TSource>(this SqlParameterBinder<TSource> binder, TSource? source)
        where TSource : notnull
    {
        return new SqlParameterBinderExecutor<TSource>( binder, source );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncMultiDataReader"/> instance.
    /// </summary>
    /// <param name="reader">Source data reader.</param>
    /// <returns>New <see cref="SqlAsyncMultiDataReader"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncMultiDataReader MultiAsync(this IDataReader reader)
    {
        return new SqlAsyncMultiDataReader( reader );
    }

    /// <summary>
    /// Creates a new <see cref="SqlAsyncMultiDataReader"/> instance.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a new <see cref="SqlAsyncMultiDataReader"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlAsyncMultiDataReader> MultiQueryAsync(
        this IDbCommand command,
        CancellationToken cancellationToken = default)
    {
        return (await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false )).MultiAsync();
    }

    /// <summary>
    /// Asynchronously reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlAsyncQueryReader"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlQueryResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncQueryReader reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        await using var r = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, options, cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// Asynchronously reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlAsyncQueryReader{TRow}"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlQueryResult<TRow>> QueryAsync<TRow>(
        this IDbCommand command,
        SqlAsyncQueryReader<TRow> reader,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRow : notnull
    {
        await using var r = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, options, cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// Asynchronously reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlAsyncQueryReaderExecutor"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlQueryResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncQueryReaderExecutor executor,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync( command, options, cancellationToken );
    }

    /// <summary>
    /// Asynchronously reads a collection of rows.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlAsyncQueryReaderExecutor{TRow}"/> to use for reading.</param>
    /// <param name="options">Query reader options.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a collection of read rows.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlQueryResult<TRow>> QueryAsync<TRow>(
        this IDbCommand command,
        SqlAsyncQueryReaderExecutor<TRow> executor,
        SqlQueryReaderOptions? options = null,
        CancellationToken cancellationToken = default)
        where TRow : notnull
    {
        return executor.ExecuteAsync( command, options, cancellationToken );
    }

    /// <summary>
    /// Asynchronously reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlAsyncScalarQueryReader"/> to use for reading.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlScalarQueryResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncScalarQueryReader reader,
        CancellationToken cancellationToken = default)
    {
        await using var r = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// Asynchronously reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="reader"><see cref="SqlAsyncScalarQueryReader{T}"/> to use for reading.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<SqlScalarQueryResult<T>> QueryAsync<T>(
        this IDbCommand command,
        SqlAsyncScalarQueryReader<T> reader,
        CancellationToken cancellationToken = default)
    {
        await using var r = await (( DbCommand )command).ExecuteReaderAsync( cancellationToken ).ConfigureAwait( false );
        return await reader.ReadAsync( r, cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// Asynchronously reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlAsyncScalarQueryReaderExecutor"/> to use for reading.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlScalarQueryResult> QueryAsync(
        this IDbCommand command,
        SqlAsyncScalarQueryReaderExecutor executor,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync( command, cancellationToken );
    }

    /// <summary>
    /// Asynchronously reads a scalar value.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="executor"><see cref="SqlAsyncScalarQueryReaderExecutor{T}"/> to use for reading.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a read scalar value.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static ValueTask<SqlScalarQueryResult<T>> QueryAsync<T>(
        this IDbCommand command,
        SqlAsyncScalarQueryReaderExecutor<T> executor,
        CancellationToken cancellationToken = default)
    {
        return executor.ExecuteAsync( command, cancellationToken );
    }

    /// <summary>
    /// Asynchronously executes the provided <paramref name="command"/>.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the number of rows affected.</returns>
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static async ValueTask<int> ExecuteAsync(this IDbCommand command, CancellationToken cancellationToken = default)
    {
        return await (( DbCommand )command).ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlAsyncQueryReader"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <returns>New <see cref="SqlAsyncQueryReaderExecutor"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryReaderExecutor Bind(this SqlAsyncQueryReader reader, string sql)
    {
        return new SqlAsyncQueryReaderExecutor( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlAsyncQueryReader{TRow}"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <typeparam name="TRow">Row type.</typeparam>
    /// <returns>New <see cref="SqlAsyncQueryReaderExecutor{TRow}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncQueryReaderExecutor<TRow> Bind<TRow>(this SqlAsyncQueryReader<TRow> reader, string sql)
        where TRow : notnull
    {
        return new SqlAsyncQueryReaderExecutor<TRow>( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlAsyncScalarQueryReader"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source scalar query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <returns>New <see cref="SqlAsyncScalarQueryReaderExecutor"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarQueryReaderExecutor Bind(this SqlAsyncScalarQueryReader reader, string sql)
    {
        return new SqlAsyncScalarQueryReaderExecutor( reader, sql );
    }

    /// <summary>
    /// Binds the provided <see cref="SqlAsyncScalarQueryReader{T}"/> to the given <paramref name="sql"/>.
    /// </summary>
    /// <param name="reader">Source scalar query reader.</param>
    /// <param name="sql">SQL to bind with.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>New <see cref="SqlAsyncScalarQueryReaderExecutor{T}"/> instance.</returns>
    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    public static SqlAsyncScalarQueryReaderExecutor<T> Bind<T>(this SqlAsyncScalarQueryReader<T> reader, string sql)
    {
        return new SqlAsyncScalarQueryReaderExecutor<T>( reader, sql );
    }
}
