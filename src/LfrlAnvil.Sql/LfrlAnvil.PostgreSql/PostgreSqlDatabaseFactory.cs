using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using LfrlAnvil.Extensions;
using LfrlAnvil.PostgreSql.Exceptions;
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

/// <summary>
/// Represents a factory of SQL databases.
/// </summary>
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlDatabaseFactory : SqlDatabaseFactory<PostgreSqlDatabase>
{
    /// <summary>
    /// Creates a new <see cref="PostgreSqlDatabaseFactory"/> instance.
    /// </summary>
    /// <param name="options">
    /// Optional <see cref="PostgreSqlDatabaseFactoryOptions"/>. Equal to <see cref="PostgreSqlDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    public PostgreSqlDatabaseFactory(PostgreSqlDatabaseFactoryOptions? options = null)
        : base( PostgreSqlDialect.Instance )
    {
        Options = options ?? PostgreSqlDatabaseFactoryOptions.Default;
    }

    /// <summary>
    /// <see cref="PostgreSqlDatabaseFactoryOptions"/> instance associated with this factory that contains DB creation options.
    /// </summary>
    public PostgreSqlDatabaseFactoryOptions Options { get; }

    /// <inheritdoc />
    [Pure]
    protected override NpgsqlConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
        var result = new NpgsqlConnectionStringBuilder( connectionString );
        if ( string.IsNullOrEmpty( result.Database ) )
            throw new InvalidOperationException( Resources.ConnectionStringMustIncludeDatabase );

        return result;
    }

    /// <inheritdoc />
    [Pure]
    protected override NpgsqlConnection CreateConnection(DbConnectionStringBuilder connectionString)
    {
        var pgsqlConnectionString = ReinterpretCast.To<NpgsqlConnectionStringBuilder>( connectionString );
        var database = pgsqlConnectionString.Database;
        pgsqlConnectionString.Database = null;
        var result = new NpgsqlConnection( pgsqlConnectionString.ToString() );
        pgsqlConnectionString.Database = database;
        return result;
    }

    /// <inheritdoc />
    protected override PostgreSqlDatabaseBuilder CreateDatabaseBuilder(string defaultSchemaName, DbConnection connection)
    {
        var serverVersion = connection.ServerVersion;
        var defaultNames = Options.DefaultNamesCreator( serverVersion, defaultSchemaName );
        var dataTypes = new PostgreSqlDataTypeProvider();
        var typeDefinitions = Options.TypeDefinitionsCreator( serverVersion, dataTypes );
        var nodeInterpreters = Options.NodeInterpretersCreator( serverVersion, defaultSchemaName, dataTypes, typeDefinitions );

        var result = new PostgreSqlDatabaseBuilder(
            connection.ServerVersion,
            defaultSchemaName,
            defaultNames,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            Options.VirtualGeneratedColumnStorageResolution );

        result.AddConnectionChangeCallback( InitializeSessionSqlMode );
        return result;
    }

    /// <inheritdoc />
    protected override PostgreSqlDatabase CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        DbConnectionEventHandler eventHandler,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version)
    {
        return new PostgreSqlDatabase(
            ReinterpretCast.To<PostgreSqlDatabaseBuilder>( builder ),
            new PostgreSqlDatabaseConnector( ReinterpretCast.To<NpgsqlConnectionStringBuilder>( connectionString ), eventHandler ),
            version,
            versionHistoryRecordsQuery );
    }

    /// <inheritdoc />
    protected override void FinalizeConnectionPreparations(
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        SqlNodeInterpreter nodeInterpreter,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var npgsqlConnectionString = ReinterpretCast.To<NpgsqlConnectionStringBuilder>( connectionString );
        var npgsqlConnection = ReinterpretCast.To<NpgsqlConnection>( connection );
        Assume.IsNotNull( npgsqlConnectionString.Database );

        var pgDatabase = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "pg_database" ) );
        var pgDatName = pgDatabase.GetRawField( "datname", TypeNullability.Create<string>() );

        var query = pgDatabase.ToDataSource()
            .AndWhere( pgDatName == SqlNode.Literal( npgsqlConnectionString.Database ) )
            .Exists()
            .As( "result" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );

        using ( var command = npgsqlConnection.CreateCommand() )
        {
            command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
            nodeInterpreter.Context.Clear();

            var exists = executor.ExecuteForVersionHistory(
                command,
                SqlHelpers.ExecuteBoolScalarDelegate,
                SqlDatabaseFactoryStatementType.Other );

            if ( ! exists )
            {
                PostgreSqlHelpers.AppendCreateDatabase(
                    nodeInterpreter,
                    npgsqlConnectionString.Database,
                    Options.EncodingName,
                    Options.LocaleName,
                    Options.ConcurrentConnectionsLimit );

                command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
                nodeInterpreter.Context.Clear();
                executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteNonQueryDelegate, SqlDatabaseFactoryStatementType.Other );
            }
        }

        npgsqlConnection.ChangeDatabase( npgsqlConnectionString.Database );
    }

    private static bool CheckCommonSchemaExistenceAndPrepare(
        PostgreSqlDatabaseChangeTracker changeTracker,
        DbConnection connection,
        string schemaName,
        SqlNodeInterpreter nodeInterpreter,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var pgNamespace = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "pg_catalog", "pg_namespace" ) );
        var nspName = pgNamespace.GetRawField( "nspname", TypeNullability.Create<string>() );

        var query = pgNamespace.ToDataSource()
            .AndWhere( nspName == SqlNode.Literal( schemaName ) )
            .Exists()
            .As( "result" )
            .ToQuery();

        nodeInterpreter.VisitDataSourceQuery( query );

        using var command = connection.CreateCommand();
        command.CommandText = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        var exists = executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteBoolScalarDelegate );
        if ( exists )
            return true;

        changeTracker.AddCreateSchemaAction( schemaName );
        return false;
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
            ReinterpretCast.To<PostgreSqlDatabaseChangeTracker>( changeTracker ),
            connection,
            versionHistoryTableName.Schema,
            nodeInterpreter,
            ref executor ) )
            return true;

        var pgNamespace = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "pg_catalog", "pg_namespace" ) );
        var nspName = pgNamespace.GetRawField( "nspname", TypeNullability.Create<string>() );
        var nspOid = pgNamespace.GetRawField( "oid", TypeNullability.Create<long>() );

        var pgClass = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "pg_catalog", "pg_class" ) );
        var tableName = pgClass.GetRawField( "relname", TypeNullability.Create<string>() );
        var tableNamespace = pgClass.GetRawField( "relnamespace", TypeNullability.Create<long>() );
        var relKind = pgClass.GetRawField( "relkind", TypeNullability.Create<string>() );

        var query = pgClass
            .Join( pgNamespace.InnerOn( nspOid == tableNamespace ) )
            .AndWhere( nspName == SqlNode.Literal( versionHistoryTableName.Schema ) )
            .AndWhere( tableName == SqlNode.Literal( versionHistoryTableName.Object ) )
            .AndWhere( relKind == SqlNode.Literal( "r" ) )
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
        return PostgreSqlHelpers.DefaultVersionHistoryName;
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
        columns.Create( SqlHelpers.VersionHistoryCommitDateUtcName ).SetType( PostgreSqlDataType.TimestampTz );
        columns.Create( SqlHelpers.VersionHistoryCommitDurationInTicksName ).SetType<long>();
    }

    private static void InitializeSessionSqlMode(SqlDatabaseConnectionChangeEvent @event)
    {
        if ( @event.StateChange.CurrentState != ConnectionState.Open )
            return;

        var connection = ReinterpretCast.To<NpgsqlConnection>( @event.Connection );

        using var command = connection.CreateCommand();
        command.CommandText = "SET standard_conforming_strings = on;SET backslash_quote = off;";
        command.ExecuteNonQuery();
    }
}
