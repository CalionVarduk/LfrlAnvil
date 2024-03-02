using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.TestExtensions.Sql.Mocks.System;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlDatabaseFactoryMock : SqlDatabaseFactory<SqlDatabaseMock>
{
    private readonly bool _createVersionHistoryTable;

    public SqlDatabaseFactoryMock(
        bool createVersionHistoryTable = false,
        string serverVersion = "0.0.0")
        : base( SqlDialectMock.Instance )
    {
        _createVersionHistoryTable = createVersionHistoryTable;
        Connection = new DbConnectionMock( serverVersion );
    }

    public DbConnectionMock Connection { get; }

    public List<Action<SqlDatabaseConnectionChangeEvent>> ConnectionChangeCallbacks { get; } =
        new List<Action<SqlDatabaseConnectionChangeEvent>>();

    [Pure]
    protected override DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
        return new DbConnectionStringBuilder { ConnectionString = connectionString };
    }

    [Pure]
    protected override DbConnectionMock CreateConnection(DbConnectionStringBuilder connectionString)
    {
        Connection.ConnectionString = connectionString.ToString();
        return Connection;
    }

    protected override SqlDatabaseBuilderMock CreateDatabaseBuilder(
        string defaultSchemaName,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var result = SqlDatabaseBuilderMock.Create( connection.ServerVersion, defaultSchemaName );
        foreach ( var callback in ConnectionChangeCallbacks )
            result.AddConnectionChangeCallback( callback );

        return result;
    }

    protected override SqlDatabaseMock CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> connectionChangeCallbacks,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        return new SqlDatabaseMock( ReinterpretCast.To<SqlDatabaseBuilderMock>( builder ), connectionString, version );
    }

    [Pure]
    protected override SqlSchemaObjectName GetDefaultVersionHistoryName()
    {
        return SqlSchemaObjectName.Create( "common", "__VersionHistory" );
    }

    protected override bool GetChangeTrackerAttachmentForVersionHistoryTableInit(
        SqlDatabaseChangeTracker changeTracker,
        SqlSchemaObjectName versionHistoryTableName,
        SqlNodeInterpreter nodeInterpreter,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        return _createVersionHistoryTable;
    }
}
