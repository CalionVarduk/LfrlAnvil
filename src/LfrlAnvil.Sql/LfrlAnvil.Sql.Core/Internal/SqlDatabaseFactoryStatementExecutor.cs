using System;
using System.Data.Common;
using System.Runtime.CompilerServices;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql.Internal;

/// <summary>
/// Represents an executor of SQL statements ran by <see cref="ISqlDatabaseFactory"/> instances.
/// Objects of this type invoke all <see cref="ISqlDatabaseFactoryStatementListener"/> instances
/// specified in <see cref="SqlCreateDatabaseOptions"/>.
/// </summary>
public struct SqlDatabaseFactoryStatementExecutor
{
    private readonly DbCommandDiagnoser<DbCommand, SqlDatabaseFactoryStatementEvent> _diagnoser;
    private SqlDatabaseFactoryStatementKey _versionHistoryKey;

    internal SqlDatabaseFactoryStatementExecutor(SqlCreateDatabaseOptions options)
    {
        _versionHistoryKey = SqlDatabaseFactoryStatementKey.Create( SqlDatabaseVersionHistory.InitialVersion ).NextOrdinal();
        CommandTimeout = options.CommandTimeout;
        var listeners = options.GetStatementListeners();

        if ( listeners.Length == 0 )
            _diagnoser = new DbCommandDiagnoser<DbCommand, SqlDatabaseFactoryStatementEvent>();
        else
        {
            var materializedListeners = options.GetStatementListeners().ToArray();
            _diagnoser = new DbCommandDiagnoser<DbCommand, SqlDatabaseFactoryStatementEvent>(
                beforeExecute: (_, @event) => BeforeExecute( materializedListeners, @event ),
                afterExecute: (_, @event, elapsedTime, exception) =>
                    AfterExecute( materializedListeners, @event, elapsedTime, exception ) );
        }
    }

    /// <summary>
    /// Optional explicit <see cref="DbCommand.CommandTimeout"/>.
    /// </summary>
    public TimeSpan? CommandTimeout { get; }

    /// <summary>
    /// Executes provided <see cref="DbCommand"/> synchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="key"><see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies this SQL statement.</param>
    /// <param name="type"><see cref="SqlDatabaseFactoryStatementType"/> instance that specifies the type of this SQL statement.</param>
    /// <param name="invoker">Delegate that invokes the provided <paramref name="command"/> and returns the result.</param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Result of the invocation.</returns>
    public T Execute<T>(
        DbCommand command,
        SqlDatabaseFactoryStatementKey key,
        SqlDatabaseFactoryStatementType type,
        Func<DbCommand, T> invoker)
    {
        using ( new TemporaryCommandTimeout( command, CommandTimeout ) )
            return InvokeCommand( command, invoker, key, type );
    }

    /// <summary>
    /// Executes provided <see cref="DbCommand"/> synchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="key"><see cref="SqlDatabaseFactoryStatementKey"/> instance that identifies this SQL statement.</param>
    /// <param name="type"><see cref="SqlDatabaseFactoryStatementType"/> instance that specifies the type of this SQL statement.</param>
    /// <param name="action"><see cref="SqlDatabaseBuilderCommandAction"/> instance that contains this SQL statement's definition.</param>
    public void Execute(
        DbCommand command,
        SqlDatabaseFactoryStatementKey key,
        SqlDatabaseFactoryStatementType type,
        SqlDatabaseBuilderCommandAction action)
    {
        using ( new TemporaryCommandTimeout( command, action.Timeout ?? CommandTimeout ) )
        {
            action.PrepareCommand( command );
            InvokeCommand( command, action.OnExecute, key, type );
        }
    }

    /// <summary>
    /// Executes provided <see cref="DbCommand"/> synchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="invoker">Delegate that invokes the provided <paramref name="command"/> and returns the result.</param>
    /// <param name="type">
    /// <see cref="SqlDatabaseFactoryStatementType"/> instance that specifies the type of this SQL statement.
    /// Equal to <see cref="SqlDatabaseFactoryStatementType.VersionHistory"/> by default.
    /// </param>
    /// <typeparam name="T">Result type.</typeparam>
    /// <returns>Result of the invocation.</returns>
    public T ExecuteForVersionHistory<T>(
        DbCommand command,
        Func<DbCommand, T> invoker,
        SqlDatabaseFactoryStatementType type = SqlDatabaseFactoryStatementType.VersionHistory)
    {
        using ( new TemporaryCommandTimeout( command, CommandTimeout ) )
            return InvokeVersionHistoryCommand( command, invoker, type );
    }

    /// <summary>
    /// Executes provided <see cref="DbCommand"/> synchronously.
    /// </summary>
    /// <param name="command">Command to execute.</param>
    /// <param name="action"><see cref="SqlDatabaseBuilderCommandAction"/> instance that contains this SQL statement's definition.</param>
    /// <param name="type">
    /// <see cref="SqlDatabaseFactoryStatementType"/> instance that specifies the type of this SQL statement.
    /// Equal to <see cref="SqlDatabaseFactoryStatementType.VersionHistory"/> by default.
    /// </param>
    public void ExecuteForVersionHistory(
        DbCommand command,
        SqlDatabaseBuilderCommandAction action,
        SqlDatabaseFactoryStatementType type = SqlDatabaseFactoryStatementType.VersionHistory)
    {
        using ( new TemporaryCommandTimeout( command, action.Timeout ?? CommandTimeout ) )
        {
            action.PrepareCommand( command );
            InvokeVersionHistoryCommand( command, action.OnExecute, type );
        }
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T InvokeCommand<T>(
        DbCommand command,
        Func<DbCommand, T> invoker,
        SqlDatabaseFactoryStatementKey key,
        SqlDatabaseFactoryStatementType type)
    {
        var @event = SqlDatabaseFactoryStatementEvent.Create( command, key, type );
        return _diagnoser.Execute( command, @event, invoker );
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private T InvokeVersionHistoryCommand<T>(DbCommand command, Func<DbCommand, T> invoker, SqlDatabaseFactoryStatementType type)
    {
        var result = InvokeCommand( command, invoker, _versionHistoryKey, type );
        _versionHistoryKey = _versionHistoryKey.NextOrdinal();
        return result;
    }

    private static void BeforeExecute(ISqlDatabaseFactoryStatementListener[] listeners, SqlDatabaseFactoryStatementEvent @event)
    {
        foreach ( var listener in listeners )
            listener.OnBefore( @event );
    }

    private static void AfterExecute(
        ISqlDatabaseFactoryStatementListener[] listeners,
        SqlDatabaseFactoryStatementEvent @event,
        TimeSpan elapsedTime,
        Exception? exception)
    {
        foreach ( var listener in listeners )
            listener.OnAfter( @event, elapsedTime, exception );
    }
}
