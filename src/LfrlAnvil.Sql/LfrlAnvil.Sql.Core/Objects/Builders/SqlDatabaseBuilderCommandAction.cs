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
using System.Data;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Statements;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a definition of an SQL statement to be ran by an <see cref="ISqlDatabaseFactory"/>.
/// </summary>
public readonly struct SqlDatabaseBuilderCommandAction
{
    private static readonly Func<IDbCommand, object?> DefaultOnExecute = static cmd =>
    {
        cmd.ExecuteNonQuery();
        return null;
    };

    private SqlDatabaseBuilderCommandAction(
        string? sql,
        Action<IDbCommand>? onCommandSetup,
        Func<IDbCommand, object?> onExecute,
        TimeSpan? timeout)
    {
        Sql = sql;
        OnCommandSetup = onCommandSetup;
        OnExecute = onExecute;
        Timeout = timeout;
    }

    /// <summary>
    /// Underlying SQL statement.
    /// </summary>
    public string? Sql { get; }

    /// <summary>
    /// Delegate that prepares the <see cref="IDbCommand"/>. Can be used to e.g. prepare a collection of parameters.
    /// </summary>
    public Action<IDbCommand>? OnCommandSetup { get; }

    /// <summary>
    /// Delegate that executes the <see cref="IDbCommand"/>.
    /// </summary>
    public Func<IDbCommand, object?> OnExecute { get; }

    /// <summary>
    /// Optional custom <see cref="IDbCommand.CommandTimeout"/>.
    /// Overrides <see cref="SqlDatabaseFactoryStatementExecutor.CommandTimeout"/> if not null.
    /// </summary>
    public TimeSpan? Timeout { get; }

    /// <summary>
    /// Returns a string representation of this <see cref="SqlDatabaseBuilderCommandAction"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        if ( Sql is null )
            return OnCommandSetup is null ? "<Custom>" : "<Custom> <WithSetup>";

        var header = OnCommandSetup is null ? "<Sql>" : "<Sql> <WithSetup>";
        return $"{header}{Environment.NewLine}{Sql}";
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseBuilderCommandAction"/> instance.
    /// </summary>
    /// <param name="sql">Underlying SQL statement.</param>
    /// <param name="timeout">Optional custom <see cref="IDbCommand.CommandTimeout"/>. Equal to null by default.</param>
    /// <returns>New <see cref="SqlDatabaseBuilderCommandAction"/> instance.</returns>
    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql(string sql, TimeSpan? timeout = null)
    {
        return new SqlDatabaseBuilderCommandAction( sql, onCommandSetup: null, DefaultOnExecute, timeout );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseBuilderCommandAction"/> instance with bound parameters.
    /// </summary>
    /// <param name="sql">Underlying SQL statement.</param>
    /// <param name="boundParameters"><see cref="SqlParameterBinderExecutor"/> instance that represents parameters to bind.</param>
    /// <param name="timeout">Optional custom <see cref="IDbCommand.CommandTimeout"/>. Equal to null by default.</param>
    /// <returns>New <see cref="SqlDatabaseBuilderCommandAction"/> instance.</returns>
    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql(
        string sql,
        SqlParameterBinderExecutor boundParameters,
        TimeSpan? timeout = null)
    {
        return new SqlDatabaseBuilderCommandAction( sql, boundParameters.Execute, DefaultOnExecute, timeout );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseBuilderCommandAction"/> instance with bound parameters.
    /// </summary>
    /// <param name="sql">Underlying SQL statement.</param>
    /// <param name="boundParameters"><see cref="SqlParameterBinderExecutor"/> instance that represents parameters to bind.</param>
    /// <param name="timeout">Optional custom <see cref="IDbCommand.CommandTimeout"/>. Equal to null by default.</param>
    /// <typeparam name="T">Parameter source type.</typeparam>
    /// <returns>New <see cref="SqlDatabaseBuilderCommandAction"/> instance.</returns>
    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateSql<T>(
        string sql,
        SqlParameterBinderExecutor<T> boundParameters,
        TimeSpan? timeout = null)
        where T : notnull
    {
        return new SqlDatabaseBuilderCommandAction( sql, boundParameters.Execute, DefaultOnExecute, timeout );
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseBuilderCommandAction"/> instance with custom execution.
    /// </summary>
    /// <param name="onExecute">Custom delegate that executes the <see cref="IDbCommand"/>.</param>
    /// <param name="onCommandSetup">
    /// Optional delegate that prepares the <see cref="IDbCommand"/>. Can be used to e.g. prepare a collection of parameters.
    /// Equal to null by default.
    /// </param>
    /// <param name="timeout">Optional custom <see cref="IDbCommand.CommandTimeout"/>. Equal to null by default.</param>
    /// <returns>New <see cref="SqlDatabaseBuilderCommandAction"/> instance.</returns>
    [Pure]
    public static SqlDatabaseBuilderCommandAction CreateCustom(
        Action<IDbCommand> onExecute,
        Action<IDbCommand>? onCommandSetup = null,
        TimeSpan? timeout = null)
    {
        return new SqlDatabaseBuilderCommandAction(
            sql: null,
            onCommandSetup,
            cmd =>
            {
                onExecute( cmd );
                return null;
            },
            timeout );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    internal void PrepareCommand(IDbCommand command)
    {
        if ( Sql is not null )
            command.CommandText = Sql;

        if ( OnCommandSetup is not null )
            OnCommandSetup.Invoke( command );
        else
            command.Parameters.Clear();
    }
}
