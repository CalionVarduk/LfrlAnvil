using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Internal;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;
using MySqlConnector;

namespace LfrlAnvil.MySql;

/// <summary>
/// Represents a factory of SQL databases.
/// </summary>
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlDatabaseFactory : SqlDatabaseFactory<MySqlDatabase>
{
    /// <summary>
    /// Creates a new <see cref="MySqlDatabaseFactory"/> instance.
    /// </summary>
    /// <param name="options">
    /// Optional <see cref="MySqlDatabaseFactoryOptions"/>. Equal to <see cref="MySqlDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    public MySqlDatabaseFactory(MySqlDatabaseFactoryOptions? options = null)
        : base( MySqlDialect.Instance )
    {
        Options = options ?? MySqlDatabaseFactoryOptions.Default;
    }

    /// <summary>
    /// <see cref="MySqlDatabaseFactoryOptions"/> instance associated with this factory that contains DB creation options.
    /// </summary>
    public MySqlDatabaseFactoryOptions Options { get; }

    /// <inheritdoc />
    [Pure]
    protected override MySqlConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
        return new MySqlConnectionStringBuilder( connectionString )
        {
            GuidFormat = MySqlGuidFormat.None,
            AllowUserVariables = true,
            NoBackslashEscapes = true,
            Database = string.Empty
        };
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlConnection CreateConnection(DbConnectionStringBuilder connectionString)
    {
        var mySqlConnectionString = ReinterpretCast.To<MySqlConnectionStringBuilder>( connectionString );
        return new MySqlConnection( mySqlConnectionString.ToString() );
    }

    /// <inheritdoc />
    [Pure]
    protected override MySqlDatabaseBuilder CreateDatabaseBuilder(string defaultSchemaName, DbConnection connection)
    {
        var serverVersion = connection.ServerVersion;
        var defaultNames = Options.DefaultNamesCreator( serverVersion, defaultSchemaName );
        var dataTypes = new MySqlDataTypeProvider();
        var typeDefinitions = Options.TypeDefinitionsCreator( serverVersion, dataTypes );
        var nodeInterpreters = Options.NodeInterpretersCreator( serverVersion, defaultSchemaName, dataTypes, typeDefinitions );

        var result = new MySqlDatabaseBuilder(
            connection.ServerVersion,
            defaultSchemaName,
            defaultNames,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            Options.IndexFilterResolution,
            Options.CharacterSetName,
            Options.CollationName,
            Options.IsEncryptionEnabled );

        result.AddConnectionChangeCallback( InitializeSessionSqlMode );
        return result;
    }

    /// <inheritdoc />
    protected override MySqlDatabase CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        DbConnectionEventHandler eventHandler,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        return new MySqlDatabase(
            ReinterpretCast.To<MySqlDatabaseBuilder>( builder ),
            new MySqlDatabaseConnector( ReinterpretCast.To<MySqlConnectionStringBuilder>( connectionString ), eventHandler ),
            version,
            versionHistoryRecordsQuery );
    }

    private static bool CheckCommonSchemaExistenceAndPrepare(
        MySqlDatabaseChangeTracker changeTracker,
        DbConnection connection,
        string schemaName,
        SqlNodeInterpreter nodeInterpreter,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var schemata = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "information_schema", "schemata" ) );
        var schemataSchemaName = schemata.GetRawField( "schema_name", TypeNullability.Create<string>() );
        var schemaNameLiteral = SqlNode.Literal( schemaName );

        var query = schemata.ToDataSource()
            .AndWhere( schemataSchemaName == schemaNameLiteral )
            .Exists()
            .As( "result" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );

        using var command = connection.CreateCommand();
        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        var exists = executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteBoolScalarDelegate );
        if ( ! exists )
        {
            changeTracker.AddCreateSchemaAction( schemaName );
            changeTracker.AddCreateGuidFunctionAction();
            changeTracker.AddCreateDropIndexIfExistsProcedureAction();
            return false;
        }

        var routines = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "information_schema", "routines" ) );
        var routineSchema = routines.GetRawField( "routine_schema", TypeNullability.Create<string>() );
        var routineName = routines.GetRawField( "routine_name", TypeNullability.Create<string>() );
        var routineType = routines.GetRawField( "routine_type", TypeNullability.Create<string>() );
        var routinesSource = routines.ToDataSource().AndWhere( routineSchema == schemaNameLiteral );

        query = routinesSource
            .AndWhere( routineType == SqlNode.Literal( "FUNCTION" ) )
            .AndWhere( routineName == SqlNode.Literal( MySqlHelpers.GuidFunctionName ) )
            .Exists()
            .As( "result" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );
        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        exists = executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteBoolScalarDelegate );
        if ( ! exists )
            changeTracker.AddCreateGuidFunctionAction();

        query = routinesSource
            .AndWhere( routineType == SqlNode.Literal( "PROCEDURE" ) )
            .AndWhere( routineName == SqlNode.Literal( MySqlHelpers.DropIndexIfExistsProcedureName ) )
            .Exists()
            .As( "result" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );
        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        exists = executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteBoolScalarDelegate );
        if ( ! exists )
            changeTracker.AddCreateDropIndexIfExistsProcedureAction();

        return true;
    }

    /// <inheritdoc />
    protected override bool GetChangeTrackerAttachmentForVersionHistoryTableInit(
        SqlDatabaseChangeTracker changeTracker,
        SqlSchemaObjectName versionHistoryTableName,
        SqlNodeInterpreter nodeInterpreter,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        if ( ! CheckCommonSchemaExistenceAndPrepare(
            ReinterpretCast.To<MySqlDatabaseChangeTracker>( changeTracker ),
            connection,
            versionHistoryTableName.Schema,
            nodeInterpreter,
            ref executor ) )
            return true;

        var tables = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "information_schema", "tables" ) );
        var tableSchema = tables.GetRawField( "table_schema", TypeNullability.Create<string>() );
        var tableName = tables.GetRawField( "table_name", TypeNullability.Create<string>() );

        var query = tables.ToDataSource()
            .AndWhere( tableSchema == SqlNode.Literal( versionHistoryTableName.Schema ) )
            .AndWhere( tableName == SqlNode.Literal( versionHistoryTableName.Object ) )
            .Exists()
            .As( "result" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );
        var sql = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        using var command = connection.CreateCommand();
        command.CommandText = sql;
        var exists = executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteBoolScalarDelegate );
        return ! exists;
    }

    /// <inheritdoc />
    [Pure]
    protected override SqlSchemaObjectName GetDefaultVersionHistoryName()
    {
        return MySqlHelpers.DefaultVersionHistoryName;
    }

    /// <inheritdoc />
    protected override SqlDatabaseCommitVersionsContext CreateCommitVersionsContext(
        SqlParameterBinderFactory parameterBinders,
        SqlCreateDatabaseOptions options)
    {
        return new MySqlDatabaseCommitVersionsContext();
    }

    /// <inheritdoc />
    protected override void VersionHistoryTableBuilderInit(SqlTableBuilder builder)
    {
        var intType = builder.Database.TypeDefinitions.GetByType<int>();
        var columns = builder.Columns;
        columns.Create( SqlHelpers.VersionHistoryVersionMajorName ).SetType( intType );
        columns.Create( SqlHelpers.VersionHistoryVersionMinorName ).SetType( intType );
        columns.Create( SqlHelpers.VersionHistoryVersionBuildName ).SetType( intType ).MarkAsNullable();
        columns.Create( SqlHelpers.VersionHistoryVersionRevisionName ).SetType( intType ).MarkAsNullable();
        columns.Create( SqlHelpers.VersionHistoryDescriptionName ).SetType<string>();
        columns.Create( SqlHelpers.VersionHistoryCommitDateUtcName ).SetType( MySqlDataType.CreateChar( 27 ) );
        columns.Create( SqlHelpers.VersionHistoryCommitDurationInTicksName ).SetType<long>();
    }

    /// <inheritdoc />
    protected override Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<SqlDatabaseVersionRecord>>
        GetVersionHistoryRecordsQueryDelegate(SqlQueryReaderFactory queryReaders)
    {
        return static (reader, options) =>
        {
            var mySqlReader = ( MySqlDataReader )reader;
            if ( ! mySqlReader.Read() )
                return SqlQueryResult<SqlDatabaseVersionRecord>.Empty;

            var rows = options.CreateList<SqlDatabaseVersionRecord>();

            var iOrdinal = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryOrdinalName );
            var iVersionMajor = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryVersionMajorName );
            var iVersionMinor = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryVersionMinorName );
            var iVersionBuild = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryVersionBuildName );
            var iVersionRevision = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryVersionRevisionName );
            var iDescription = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryDescriptionName );
            var iCommitDateUtc = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryCommitDateUtcName );
            var iCommitDurationInTicks = mySqlReader.GetOrdinal( SqlHelpers.VersionHistoryCommitDurationInTicksName );

            do
            {
                var versionMajor = mySqlReader.GetInt32( iVersionMajor );
                var versionMinor = mySqlReader.GetInt32( iVersionMinor );
                var versionBuild = mySqlReader.IsDBNull( iVersionBuild ) ? ( int? )null : mySqlReader.GetInt32( iVersionBuild );
                var versionRevision = versionBuild is null || mySqlReader.IsDBNull( iVersionRevision )
                    ? ( int? )null
                    : mySqlReader.GetInt32( iVersionRevision );

                var record = new SqlDatabaseVersionRecord(
                    Ordinal: mySqlReader.GetInt32( iOrdinal ),
                    Version: versionBuild is null
                        ? new Version( versionMajor, versionMinor )
                        : versionRevision is null
                            ? new Version( versionMajor, versionMinor, versionBuild.Value )
                            : new Version( versionMajor, versionMinor, versionBuild.Value, versionRevision.Value ),
                    Description: mySqlReader.GetString( iDescription ),
                    CommitDateUtc: DateTime.SpecifyKind( DateTime.Parse( mySqlReader.GetString( iCommitDateUtc ) ), DateTimeKind.Utc ),
                    CommitDuration: TimeSpan.FromTicks( mySqlReader.GetInt64( iCommitDurationInTicks ) ) );

                rows.Add( record );
            }
            while ( mySqlReader.Read() );

            return new SqlQueryResult<SqlDatabaseVersionRecord>( resultSetFields: null, rows );
        };
    }

    private static void InitializeSessionSqlMode(SqlDatabaseConnectionChangeEvent @event)
    {
        if ( @event.StateChange.CurrentState != ConnectionState.Open )
            return;

        var connection = ReinterpretCast.To<MySqlConnection>( @event.Connection );

        using var command = connection.CreateCommand();
        command.CommandText = "SET SESSION sql_mode = 'ANSI,TRADITIONAL,NO_BACKSLASH_ESCAPES';";
        command.ExecuteNonQuery();
    }
}
