using System.Collections.Generic;
using System.Data.Common;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqliteDatabaseCommitVersionsContext : SqlDatabaseCommitVersionsContext
{
    private readonly bool _disableForeignKeyChecks;
    private DbCommand? _preparePragmaCommand;
    private DbCommand? _restorePragmaCommand;
    private bool _isPragmaSwapped;

    internal SqliteDatabaseCommitVersionsContext(bool disableForeignKeyChecks)
    {
        _disableForeignKeyChecks = disableForeignKeyChecks;
    }

    public override void Dispose()
    {
        base.Dispose();
        _preparePragmaCommand?.Dispose();
        _restorePragmaCommand?.Dispose();
    }

    protected override void OnBeforeVersionRangeApplication(
        SqlDatabaseBuilder builder,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        base.OnBeforeVersionRangeApplication( builder, connection, ref executor );
        Assume.IsNull( _preparePragmaCommand );
        Assume.IsNull( _restorePragmaCommand );

        ReinterpretCast.To<SqliteDatabaseChangeTracker>( builder.Changes ).ClearModifiedTables();

        using var command = connection.CreateCommand();
        command.CommandText = "PRAGMA foreign_keys; PRAGMA legacy_alter_table;";
        var (areForeignKeysEnabled, isLegacyAlterTableEnabled) = executor.ExecuteForVersionHistory(
            command,
            static cmd =>
            {
                using var reader = cmd.ExecuteReader();
                reader.Read();
                var fkResult = reader.GetBoolean( 0 );
                reader.NextResult();
                reader.Read();
                return (fkResult, reader.GetBoolean( 0 ));
            },
            SqlDatabaseFactoryStatementType.Other );

        if ( areForeignKeysEnabled )
        {
            _preparePragmaCommand = connection.CreateCommand();
            _preparePragmaCommand.CommandText = "PRAGMA foreign_keys = 0;";
            _restorePragmaCommand = connection.CreateCommand();
            _restorePragmaCommand.CommandText = "PRAGMA foreign_keys = 1;";
        }

        if ( ! isLegacyAlterTableEnabled )
        {
            _preparePragmaCommand ??= connection.CreateCommand();
            _preparePragmaCommand.CommandText += "PRAGMA legacy_alter_table = 1;";
            _restorePragmaCommand ??= connection.CreateCommand();
            _restorePragmaCommand.CommandText += "PRAGMA legacy_alter_table = 0;";
        }
    }

    protected override SqlDatabaseFactoryStatementKey OnBeforeVersionTransaction(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        key = base.OnBeforeVersionTransaction( builder, key, connection, ref executor );
        if ( _preparePragmaCommand is null )
            return key;

        Assume.False( _isPragmaSwapped );
        key = key.NextOrdinal();
        executor.Execute( _preparePragmaCommand, key, SqlDatabaseFactoryStatementType.Other, SqlHelpers.ExecuteNonQueryDelegate );
        _isPragmaSwapped = true;
        return key;
    }

    protected override SqlDatabaseFactoryStatementKey OnAfterVersionActionRangeExecution(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbCommand command,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        key = base.OnAfterVersionActionRangeExecution( builder, key, command, ref executor );

        var changeTracker = ReinterpretCast.To<SqliteDatabaseChangeTracker>( builder.Changes );
        if ( _disableForeignKeyChecks )
        {
            changeTracker.ClearModifiedTables();
            return key;
        }

        HashSet<string>? foreignKeyCheckFailures = null;
        foreach ( var table in changeTracker.ModifiedTables )
        {
            var tableName = SqliteHelpers.GetFullName( table.Schema.Name, table.Name );
            key = key.NextOrdinal();

            command.Parameters.Clear();
            command.CommandText = $"PRAGMA foreign_key_check('{tableName}');";

            var hasFkFailure = executor.Execute(
                command,
                key,
                SqlDatabaseFactoryStatementType.Other,
                static cmd =>
                {
                    using var reader = cmd.ExecuteReader();
                    return reader.Read();
                } );

            if ( ! hasFkFailure )
                continue;

            foreignKeyCheckFailures ??= new HashSet<string>( SqlHelpers.NameComparer );
            foreignKeyCheckFailures.Add( tableName );
        }

        changeTracker.ClearModifiedTables();
        if ( foreignKeyCheckFailures is not null )
            throw new SqliteForeignKeyCheckException( key.Version, foreignKeyCheckFailures );

        return key;
    }

    protected override void OnAfterVersionTransaction(
        SqlDatabaseBuilder builder,
        SqlDatabaseFactoryStatementKey key,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        base.OnAfterVersionTransaction( builder, key, connection, ref executor );
        if ( ! _isPragmaSwapped || _restorePragmaCommand is null )
            return;

        key = key.NextOrdinal();
        executor.Execute( _restorePragmaCommand, key, SqlDatabaseFactoryStatementType.Other, SqlHelpers.ExecuteNonQueryDelegate );
        _isPragmaSwapped = false;
    }
}
