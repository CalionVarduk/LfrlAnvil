using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteDatabaseFactory : SqlDatabaseFactory<SqliteDatabase>
{
    public SqliteDatabaseFactory(bool isConnectionPermanent = false)
        : base( SqliteDialect.Instance )
    {
        IsConnectionPermanent = isConnectionPermanent;
    }

    public bool IsConnectionPermanent { get; }

    [Pure]
    protected override SqliteConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
        return new SqliteConnectionStringBuilder( connectionString );
    }

    [Pure]
    protected override SqliteConnection CreateConnection(DbConnectionStringBuilder connectionString)
    {
        var sqliteConnectionString = ReinterpretCast.To<SqliteConnectionStringBuilder>( connectionString );
        return IsConnectionPermanent || sqliteConnectionString.DataSource == SqliteHelpers.MemoryDataSource
            ? new SqlitePermanentConnection( sqliteConnectionString.ToString() )
            : new SqliteConnection( sqliteConnectionString.ToString() );
    }

    protected override SqliteDatabaseBuilder CreateDatabaseBuilder(
        string defaultSchemaName,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var builder = new SqliteColumnTypeDefinitionProviderBuilder();
        var result = new SqliteDatabaseBuilder( connection.ServerVersion, defaultSchemaName, builder.Build() );
        result.AddConnectionChangeCallback( FunctionInitializer );
        return result;
    }

    protected override SqliteDatabase CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> connectionChangeCallbacks,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        var sqliteConnectionString = ReinterpretCast.To<SqliteConnectionStringBuilder>( connectionString );
        var sqliteBuilder = ReinterpretCast.To<SqliteDatabaseBuilder>( builder );

        return connection is SqlitePermanentConnection permanentConnection
            ? new SqlitePermanentlyConnectedDatabase(
                permanentConnection,
                sqliteConnectionString,
                sqliteBuilder,
                version,
                versionHistoryRecordsQuery,
                connectionChangeCallbacks )
            : new SqlitePersistentDatabase(
                connection.ConnectionString,
                sqliteConnectionString,
                sqliteBuilder,
                version,
                versionHistoryRecordsQuery,
                connectionChangeCallbacks );
    }

    [Pure]
    protected override SqlSchemaObjectName GetDefaultVersionHistoryName()
    {
        return SqliteHelpers.DefaultVersionHistoryName;
    }

    protected override bool GetChangeTrackerAttachmentForVersionHistoryTableInit(
        SqlDatabaseChangeTracker changeTracker,
        SqlSchemaObjectName versionHistoryTableName,
        SqlNodeInterpreter nodeInterpreter,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var sqliteMaster = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "sqlite_master" ) );
        var sqliteMasterType = sqliteMaster.GetRawField( "type", TypeNullability.Create<string>() );
        var sqliteMasterName = sqliteMaster.GetRawField( "name", TypeNullability.Create<string>() );
        var fullVersionHistoryTableName = SqliteHelpers.GetFullName( versionHistoryTableName.Schema, versionHistoryTableName.Object );

        var query = sqliteMaster.ToDataSource()
            .AndWhere( sqliteMasterType == SqlNode.Literal( "table" ) )
            .AndWhere( sqliteMasterName == SqlNode.Literal( fullVersionHistoryTableName ) )
            .Exists()
            .As( "exists" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );
        var sql = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var exists = executor.ExecuteForVersionHistory( command, static cmd => Convert.ToBoolean( cmd.ExecuteScalar() ) );
        return ! exists;
    }

    protected override void OnUncaughtException(Exception exception, DbConnection connection)
    {
        base.OnUncaughtException( exception, connection );
        if ( connection is SqlitePermanentConnection )
            connection.Close();
    }

    protected override SqlDatabaseCommitVersionsContext CreateCommitVersionsContext(
        SqlParameterBinderFactory parameterBinders,
        SqlCreateDatabaseOptions options)
    {
        return new SqliteDatabaseCommitVersionsContext();
    }

    private static void FunctionInitializer(SqlDatabaseConnectionChangeEvent @event)
    {
        if ( @event.StateChange.CurrentState != ConnectionState.Open )
            return;

        var connection = ReinterpretCast.To<SqliteConnection>( @event.Connection );

        using ( var command = connection.CreateCommand() )
        {
            command.CommandText = "PRAGMA foreign_keys = 1; PRAGMA ignore_check_constraints = 0;";
            command.ExecuteNonQuery();
        }

        connection.CreateFunction( "GET_CURRENT_DATE", SqliteHelpers.DbGetCurrentDate );
        connection.CreateFunction( "GET_CURRENT_TIME", SqliteHelpers.DbGetCurrentTime );
        connection.CreateFunction( "GET_CURRENT_DATETIME", SqliteHelpers.DbGetCurrentDateTime );
        connection.CreateFunction( "GET_CURRENT_TIMESTAMP", SqliteHelpers.DbGetCurrentTimestamp );
        connection.CreateFunction( "NEW_GUID", SqliteHelpers.DbNewGuid );
        connection.CreateFunction<string?, string?>( "TO_LOWER", SqliteHelpers.DbToLower, isDeterministic: true );
        connection.CreateFunction<string?, string?>( "TO_UPPER", SqliteHelpers.DbToUpper, isDeterministic: true );
        connection.CreateFunction<string?, string?, long?>( "INSTR_LAST", SqliteHelpers.DbInstrLast, isDeterministic: true );
        connection.CreateFunction<string?, string?>( "REVERSE", SqliteHelpers.DbReverse, isDeterministic: true );
        connection.CreateFunction<double?, int?, double?>( "TRUNC2", SqliteHelpers.DbTrunc2, isDeterministic: true );
        connection.CreateFunction<string?, string?>( "TIME_OF_DAY", SqliteHelpers.DbTimeOfDay, isDeterministic: true );
        connection.CreateFunction<long?, string?, long?>( "EXTRACT_TEMPORAL", SqliteHelpers.DbExtractTemporal, isDeterministic: true );
        connection.CreateFunction<long?, long?, string?, string?>( "TEMPORAL_ADD", SqliteHelpers.DbTemporalAdd, isDeterministic: true );
        connection.CreateFunction<long?, string?, string?, long?>( "TEMPORAL_DIFF", SqliteHelpers.DbTemporalDiff, isDeterministic: true );
    }
}
