using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
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
    public SqliteDatabaseFactory(SqliteDatabaseFactoryOptions? options = null)
        : base( SqliteDialect.Instance )
    {
        Options = options ?? SqliteDatabaseFactoryOptions.Default;
    }

    public SqliteDatabaseFactoryOptions Options { get; }

    [Pure]
    protected override SqliteConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
        return new SqliteConnectionStringBuilder( connectionString );
    }

    [Pure]
    protected override SqliteConnection CreateConnection(DbConnectionStringBuilder connectionString)
    {
        var sqliteConnectionString = ReinterpretCast.To<SqliteConnectionStringBuilder>( connectionString );
        return Options.IsConnectionPermanent || sqliteConnectionString.DataSource == SqliteHelpers.MemoryDataSource
            ? new SqlitePermanentConnection( sqliteConnectionString.ToString() )
            : new SqliteConnection( sqliteConnectionString.ToString() );
    }

    protected override SqliteDatabaseBuilder CreateDatabaseBuilder(
        string defaultSchemaName,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var serverVersion = connection.ServerVersion;
        var defaultNames = Options.DefaultNamesCreator( serverVersion, defaultSchemaName );
        var dataTypes = new SqliteDataTypeProvider();
        var typeDefinitions = Options.TypeDefinitionsCreator( serverVersion, dataTypes );
        var nodeInterpreters = Options.NodeInterpretersCreator( serverVersion, defaultSchemaName, dataTypes, typeDefinitions );

        var result = new SqliteDatabaseBuilder(
            serverVersion,
            defaultSchemaName,
            defaultNames,
            dataTypes,
            typeDefinitions,
            nodeInterpreters );

        result.AddConnectionChangeCallback( FunctionInitializer );
        return result;
    }

    protected override SqliteDatabase CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        DbConnectionEventHandler eventHandler,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        var sqliteConnectionString = ReinterpretCast.To<SqliteConnectionStringBuilder>( connectionString );
        var sqliteBuilder = ReinterpretCast.To<SqliteDatabaseBuilder>( builder );

        return connection is SqlitePermanentConnection permanentConnection
            ? CreateSqliteDatabaseWithPermanentConnection(
                sqliteBuilder,
                sqliteConnectionString,
                permanentConnection,
                eventHandler,
                versionHistoryRecordsQuery,
                version )
            : CreateSqliteDatabase(
                sqliteBuilder,
                sqliteConnectionString,
                eventHandler,
                versionHistoryRecordsQuery,
                version );
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
        return new SqliteDatabaseCommitVersionsContext( Options.AreForeignKeyChecksDisabled );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqliteDatabase CreateSqliteDatabaseWithPermanentConnection(
        SqliteDatabaseBuilder builder,
        SqliteConnectionStringBuilder connectionStringBuilder,
        SqlitePermanentConnection connection,
        DbConnectionEventHandler eventHandler,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        var connector = new SqliteDatabasePermanentConnector( connection, connectionStringBuilder, eventHandler );
        var result = new SqliteDatabase( builder, connector, version, versionHistoryRecordsQuery );
        connector.SetDatabase( result );
        return result;
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqliteDatabase CreateSqliteDatabase(
        SqliteDatabaseBuilder builder,
        SqliteConnectionStringBuilder connectionStringBuilder,
        DbConnectionEventHandler eventHandler,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        var connector = new SqliteDatabaseConnector( connectionStringBuilder, eventHandler );
        var result = new SqliteDatabase( builder, connector, version, versionHistoryRecordsQuery );
        connector.SetDatabase( result );
        return result;
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
        connection.CreateFunction( "GET_CURRENT_UTC_DATETIME", SqliteHelpers.DbGetCurrentUtcDateTime );
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
