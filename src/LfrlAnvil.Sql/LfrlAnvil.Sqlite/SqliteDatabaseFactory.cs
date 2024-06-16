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

/// <summary>
/// Represents a factory of SQL databases.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteDatabaseFactory : SqlDatabaseFactory<SqliteDatabase>
{
    /// <summary>
    /// Creates a new <see cref="SqliteDatabaseFactory"/> instance.
    /// </summary>
    /// <param name="options">
    /// Optional <see cref="SqliteDatabaseFactoryOptions"/>. Equal to <see cref="SqliteDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    public SqliteDatabaseFactory(SqliteDatabaseFactoryOptions? options = null)
        : base( SqliteDialect.Instance )
    {
        Options = options ?? SqliteDatabaseFactoryOptions.Default;
    }

    /// <summary>
    /// <see cref="SqliteDatabaseFactoryOptions"/> instance associated with this factory that contains DB creation options.
    /// </summary>
    public SqliteDatabaseFactoryOptions Options { get; }

    /// <inheritdoc />
    [Pure]
    protected override SqliteConnectionStringBuilder CreateConnectionStringBuilder(string connectionString)
    {
        return new SqliteConnectionStringBuilder( connectionString );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteConnection CreateConnection(DbConnectionStringBuilder connectionString)
    {
        var sqliteConnectionString = ReinterpretCast.To<SqliteConnectionStringBuilder>( connectionString );
        return Options.IsConnectionPermanent || sqliteConnectionString.DataSource == SqliteHelpers.MemoryDataSource
            ? new SqlitePermanentConnection( sqliteConnectionString.ToString() )
            : new SqliteConnection( sqliteConnectionString.ToString() );
    }

    /// <inheritdoc />
    [Pure]
    protected override SqliteDatabaseBuilder CreateDatabaseBuilder(string defaultSchemaName, DbConnection connection)
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

    /// <inheritdoc />
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

    /// <inheritdoc />
    [Pure]
    protected override SqlSchemaObjectName GetDefaultVersionHistoryName()
    {
        return SqliteHelpers.DefaultVersionHistoryName;
    }

    /// <inheritdoc />
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
        var exists = executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteBoolScalarDelegate );

        if ( Options.Encoding is not null && ! exists )
            SetEncoding( command, Options.Encoding.Value, ref executor );

        return ! exists;
    }

    /// <inheritdoc />
    protected override void OnUncaughtException(Exception exception, DbConnection connection)
    {
        base.OnUncaughtException( exception, connection );
        if ( connection is SqlitePermanentConnection )
            connection.Close();
    }

    /// <inheritdoc />
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

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void SetEncoding(DbCommand command, SqliteDatabaseEncoding encoding, ref SqlDatabaseFactoryStatementExecutor executor)
    {
        var encodingName = encoding switch
        {
            SqliteDatabaseEncoding.UTF_8 => "UTF-8",
            SqliteDatabaseEncoding.UTF_16 => "UTF-16",
            SqliteDatabaseEncoding.UTF_16_LE => "UTF-16le",
            _ => "UTF-16be"
        };

        command.CommandText = $"PRAGMA encoding = '{encodingName}';";
        executor.ExecuteForVersionHistory( command, SqlHelpers.ExecuteNonQueryDelegate );
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
