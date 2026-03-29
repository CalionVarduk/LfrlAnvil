// Copyright 2026 Łukasz Furlepa
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

using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace LfrlAnvil.Sql.Statements;

/// <summary>
/// Represents an <see cref="SqlParameterBinder"/> bound to a specific SQL statement.
/// </summary>
/// <param name="ParameterBinder">Underlying parameter binder.</param>
/// <param name="Sql">Bound SQL statement.</param>
public readonly record struct SqlParameterizedStatementExecutor(SqlParameterBinder ParameterBinder, string Sql)
{
    /// <summary>
    /// Creates a new parameterized <see cref="SqlMultiDataReader"/> instance.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <returns>New <see cref="SqlMultiDataReader"/> instance.</returns>
    public SqlMultiDataReader MultiQuery(IDbCommand command, IEnumerable<SqlParameter>? parameters)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.MultiQuery();
    }

    /// <summary>
    /// Asynchronously creates a new parameterized <see cref="SqlAsyncMultiDataReader"/> instance.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a new <see cref="SqlAsyncMultiDataReader"/> instance.</returns>
    public ValueTask<SqlAsyncMultiDataReader> MultiQueryAsync(
        IDbCommand command,
        IEnumerable<SqlParameter>? parameters,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.MultiQueryAsync( cancellationToken );
    }

    /// <summary>
    /// Executes the <see cref="Sql"/> statement with the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <returns>The number of rows affected.</returns>
    public int Execute(IDbCommand command, IEnumerable<SqlParameter>? parameters)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.Execute();
    }

    /// <summary>
    /// Asynchronously executes the <see cref="Sql"/> statement with the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the number of rows affected.</returns>
    public ValueTask<int> ExecuteAsync(
        IDbCommand command,
        IEnumerable<SqlParameter>? parameters,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.ExecuteAsync( cancellationToken );
    }

    /// <summary>
    /// Executes the <see cref="Sql"/> statement for each parameter source in the <paramref name="parameters"/> collection.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Collection of sources of parameters to bind.</param>
    /// <returns>The number of rows affected.</returns>
    public long Execute(IDbCommand command, IEnumerable<IEnumerable<SqlParameter>?> parameters)
    {
        var result = 0L;
        command.CommandText = Sql;
        foreach ( var p in parameters )
        {
            ParameterBinder.Bind( command, p );
            result += command.Execute();
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes the <see cref="Sql"/> statement for each parameter source in the <paramref name="parameters"/> collection.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Collection of sources of parameters to bind.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the number of rows affected.</returns>
    public async ValueTask<long> ExecuteAsync(
        IDbCommand command,
        IEnumerable<IEnumerable<SqlParameter>?> parameters,
        CancellationToken cancellationToken = default)
    {
        var result = 0L;
        var cmd = ( DbCommand )command;
        cmd.CommandText = Sql;
        foreach ( var p in parameters )
        {
            ParameterBinder.Bind( cmd, p );
            result += await cmd.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
        }

        return result;
    }
}

/// <summary>
/// Represents an <see cref="SqlParameterBinder{TSource}"/> bound to a specific SQL statement.
/// </summary>
/// <param name="ParameterBinder">Underlying parameter binder.</param>
/// <param name="Sql">Bound SQL statement.</param>
/// <typeparam name="T">Parameter source type.</typeparam>
public readonly record struct SqlParameterizedStatementExecutor<T>(SqlParameterBinder<T> ParameterBinder, string Sql)
    where T : notnull
{
    /// <summary>
    /// Creates a new parameterized <see cref="SqlMultiDataReader"/> instance.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <returns>New <see cref="SqlMultiDataReader"/> instance.</returns>
    public SqlMultiDataReader MultiQuery(IDbCommand command, T? parameters)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.MultiQuery();
    }

    /// <summary>
    /// Asynchronously creates a new parameterized <see cref="SqlAsyncMultiDataReader"/> instance.
    /// </summary>
    /// <param name="command">Source command.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns a new <see cref="SqlAsyncMultiDataReader"/> instance.</returns>
    public ValueTask<SqlAsyncMultiDataReader> MultiQueryAsync(
        IDbCommand command,
        T? parameters,
        CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.MultiQueryAsync( cancellationToken );
    }

    /// <summary>
    /// Executes the <see cref="Sql"/> statement with the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <returns>The number of rows affected.</returns>
    public int Execute(IDbCommand command, T? parameters)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.Execute();
    }

    /// <summary>
    /// Asynchronously executes the <see cref="Sql"/> statement with the given <paramref name="parameters"/>.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Source of parameters to bind.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the number of rows affected.</returns>
    public ValueTask<int> ExecuteAsync(IDbCommand command, T? parameters, CancellationToken cancellationToken = default)
    {
        command.CommandText = Sql;
        ParameterBinder.Bind( command, parameters );
        return command.ExecuteAsync( cancellationToken );
    }

    /// <summary>
    /// Executes the <see cref="Sql"/> statement for each parameter source in the <paramref name="parameters"/> collection.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Collection of sources of parameters to bind.</param>
    /// <returns>The number of rows affected.</returns>
    public long Execute(IDbCommand command, IEnumerable<T?> parameters)
    {
        var result = 0L;
        command.CommandText = Sql;
        foreach ( var p in parameters )
        {
            ParameterBinder.Bind( command, p );
            result += command.Execute();
        }

        return result;
    }

    /// <summary>
    /// Asynchronously executes the <see cref="Sql"/> statement for each parameter source in the <paramref name="parameters"/> collection.
    /// </summary>
    /// <param name="command">Command to execute the statement with.</param>
    /// <param name="parameters">Collection of sources of parameters to bind.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns><see cref="ValueTask{TResult}"/> that returns the number of rows affected.</returns>
    public async ValueTask<long> ExecuteAsync(IDbCommand command, IEnumerable<T?> parameters, CancellationToken cancellationToken = default)
    {
        var result = 0L;
        var cmd = ( DbCommand )command;
        cmd.CommandText = Sql;
        foreach ( var p in parameters )
        {
            ParameterBinder.Bind( cmd, p );
            result += await cmd.ExecuteNonQueryAsync( cancellationToken ).ConfigureAwait( false );
        }

        return result;
    }
}
