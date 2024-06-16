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
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Extensions;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Statements.Compilers;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a factory of SQL databases.
/// </summary>
/// <typeparam name="TDatabase">SQL database type.</typeparam>
public abstract class SqlDatabaseFactory<TDatabase> : ISqlDatabaseFactory
    where TDatabase : SqlDatabase
{
    /// <summary>
    /// Creates a new <see cref="SqlDatabaseFactory{TDatabase}"/> instance.
    /// </summary>
    /// <param name="dialect"></param>
    protected SqlDatabaseFactory(SqlDialect dialect)
    {
        Dialect = dialect;
    }

    /// <inheritdoc />
    public SqlDialect Dialect { get; }

    /// <inheritdoc cref="ISqlDatabaseFactory.Create(String,SqlDatabaseVersionHistory,SqlCreateDatabaseOptions)" />
    public SqlCreateDatabaseResult<TDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default)
    {
        var connectionStringBuilder = CreateConnectionStringBuilder( connectionString );
        var connection = CreateConnection( connectionStringBuilder );
        Assume.NotEquals( connection.State, ConnectionState.Open );
        var stateChanges = new SqlDatabaseConnectionStateChanges();
        SqlDatabaseBuilder? builder = null;

        try
        {
            connection.StateChange += stateChanges.Add;
            connection.Open();

            var versionHistoryName = options.VersionHistoryName ?? GetDefaultVersionHistoryName();
            builder = CreateDatabaseBuilder( versionHistoryName.Schema, connection );
            stateChanges.SetBuilder( builder );

            Assume.Equals( builder.Schemas.Default.Name, versionHistoryName.Schema );
            Assume.IsEmpty( builder.Schemas.Default.Objects );
            Assume.ContainsAtLeast( builder.Schemas, 1 );

            var executor = new SqlDatabaseFactoryStatementExecutor( options );
            var nodeInterpreter = builder.NodeInterpreters.Create( SqlNodeInterpreterContext.Create( capacity: 256 ) );
            FinalizeConnectionPreparations( connectionStringBuilder, connection, nodeInterpreter, ref executor );
            Assume.Equals( connection.State, ConnectionState.Open );

            var versionHistoryTable = InitializeVersionHistoryTable(
                builder,
                versionHistoryName,
                nodeInterpreter,
                connection,
                ref executor );

            builder.Changes.SetModeAndAttach( SqlDatabaseCreateMode.NoChanges );

            var versionHistoryRecordsQuery = CreateVersionHistoryRecordsQuery( versionHistoryTable, nodeInterpreter );
            var versions = CompareVersionHistoryToDatabase(
                versionHistory,
                GetVersionHistoryRecordsQueryForDatabaseComparison(
                    versionHistoryRecordsQuery,
                    options.VersionHistoryQueryMode,
                    versionHistoryTable,
                    nodeInterpreter ),
                connection,
                ref executor );

            var mode = versions.Uncommitted.Length > 0 ? options.Mode : SqlDatabaseCreateMode.NoChanges;
            switch ( mode )
            {
                case SqlDatabaseCreateMode.Commit:
                {
                    using var context = CreateCommitVersionsContext( builder.ParameterBinders, options );
                    context.InitializeVersionHistoryCommands(
                        connection,
                        versionHistoryTable,
                        nodeInterpreter,
                        options.VersionHistoryPersistenceMode );

                    versionHistoryTable.Remove();
                    Assume.ContainsAtLeast( builder.Schemas, 1 );
                    Assume.IsEmpty( builder.Schemas.Default.Objects );

                    ApplyCommittedVersions( builder, stateChanges, versions.Committed );
                    var (exception, appliedVersionCount) = ApplyUncommittedVersionsInCommitMode(
                        builder,
                        versions,
                        stateChanges,
                        context,
                        connection,
                        ref executor );

                    var newDbVersion = appliedVersionCount > 0 ? versions.Uncommitted[appliedVersionCount - 1].Value : versions.Current;
                    return new SqlCreateDatabaseResult<TDatabase>(
                        database: CreateDatabase(
                            builder,
                            connectionStringBuilder,
                            connection,
                            stateChanges.CreateEventHandler(),
                            versionHistoryRecordsQuery,
                            newDbVersion ),
                        exception: exception,
                        versions: versions,
                        appliedVersionCount: appliedVersionCount );
                }
                case SqlDatabaseCreateMode.DryRun:
                {
                    versionHistoryTable.Remove();
                    Assume.ContainsAtLeast( builder.Schemas, 1 );
                    Assume.IsEmpty( builder.Schemas.Default.Objects );

                    ApplyCommittedVersions( builder, stateChanges, versions.Committed );
                    ApplyUncommittedVersionsInDryRunMode( builder, versions.Uncommitted );

                    return new SqlCreateDatabaseResult<TDatabase>(
                        database: CreateDatabase(
                            builder,
                            connectionStringBuilder,
                            connection,
                            stateChanges.CreateEventHandler(),
                            versionHistoryRecordsQuery,
                            versions.Current ),
                        exception: null,
                        versions: versions,
                        appliedVersionCount: 0 );
                }
                default:
                {
                    versionHistoryTable.Remove();
                    Assume.ContainsAtLeast( builder.Schemas, 1 );
                    Assume.IsEmpty( builder.Schemas.Default.Objects );

                    ApplyCommittedVersions( builder, stateChanges, versions.Committed );

                    return new SqlCreateDatabaseResult<TDatabase>(
                        database: CreateDatabase(
                            builder,
                            connectionStringBuilder,
                            connection,
                            stateChanges.CreateEventHandler(),
                            versionHistoryRecordsQuery,
                            versions.Current ),
                        exception: null,
                        versions: versions,
                        appliedVersionCount: 0 );
                }
            }
        }
        catch ( Exception exc )
        {
            OnUncaughtException( exc, connection );
            throw;
        }
        finally
        {
            connection.Dispose();
            connection.StateChange -= stateChanges.Add;
            if ( builder is not null )
                builder.UserData = null;
        }
    }

    /// <summary>
    /// Creates a new <see cref="DbConnectionStringBuilder"/> instance.
    /// </summary>
    /// <param name="connectionString">Connection string.</param>
    /// <returns>New <see cref="DbConnectionStringBuilder"/> instance.</returns>
    [Pure]
    protected abstract DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString);

    /// <summary>
    /// Creates a new unopened <see cref="DbConnection"/> instance.
    /// </summary>
    /// <param name="connectionString">Connection string builder.</param>
    /// <returns>New <see cref="DbConnection"/> instance.</returns>
    [Pure]
    protected abstract DbConnection CreateConnection(DbConnectionStringBuilder connectionString);

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseBuilder"/> instance.
    /// </summary>
    /// <param name="defaultSchemaName">
    /// Initial name of the <see cref="ISqlSchemaBuilderCollection.Default"/> schema. Version history table will belong to this schema.
    /// </param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <returns>New <see cref="SqlDatabaseBuilder"/> instance.</returns>
    [Pure]
    protected abstract SqlDatabaseBuilder CreateDatabaseBuilder(string defaultSchemaName, DbConnection connection);

    /// <summary>
    /// Creates a new <see cref="SqlDatabase"/> instance.
    /// </summary>
    /// <param name="builder">Source database builder.</param>
    /// <param name="connectionString">Connection string builder.</param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="eventHandler">Collection of <see cref="SqlDatabaseConnectionChangeEvent"/> callbacks.</param>
    /// <param name="versionHistoryRecordsQuery">
    /// Query reader's executor capable of reading metadata of all versions applied to the database.
    /// </param>
    /// <param name="version">Current version of the database.</param>
    /// <returns>New <see cref="SqlDatabase"/> instance.</returns>
    protected abstract TDatabase CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        DbConnectionEventHandler eventHandler,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version);

    /// <summary>
    /// Creates a new <see cref="SqlSchemaObjectName"/> instance that represents default version history table name.
    /// </summary>
    /// <returns>New <see cref="SqlSchemaObjectName"/> instance.</returns>
    [Pure]
    protected abstract SqlSchemaObjectName GetDefaultVersionHistoryName();

    /// <summary>
    /// Finalizes DB connection preparations. This method is invoked right before version history table initialization.
    /// </summary>
    /// <param name="connectionString">Connection string builder.</param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="nodeInterpreter"><see cref="SqlNodeInterpreter"/> instance.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    protected virtual void FinalizeConnectionPreparations(
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        SqlNodeInterpreter nodeInterpreter,
        ref SqlDatabaseFactoryStatementExecutor executor) { }

    /// <summary>
    /// Checks whether or not the version history table should be attached as a change
    /// from which an SQL statement should be created and executed.
    /// </summary>
    /// <param name="changeTracker">Database builder's change tracker.</param>
    /// <param name="versionHistoryTableName">Name of the version history table.</param>
    /// <param name="nodeInterpreter"><see cref="SqlNodeInterpreter"/> instance.</param>
    /// <param name="connection">Opened connection to the database.</param>
    /// <param name="executor">Decorator for executing SQL statements on the database.</param>
    /// <returns><b>true</b> when version history table should be created in the database, otherwise <b>false</b>.</returns>
    /// <remarks>This method can also be used for registering other common database objects.</remarks>
    protected abstract bool GetChangeTrackerAttachmentForVersionHistoryTableInit(
        SqlDatabaseChangeTracker changeTracker,
        SqlSchemaObjectName versionHistoryTableName,
        SqlNodeInterpreter nodeInterpreter,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor);

    /// <summary>
    /// Initializes the version history table builder.
    /// </summary>
    /// <param name="builder">Version history table builder.</param>
    /// <remarks>
    /// By default, this method adds the following columns:
    /// <list type="bullet">
    /// <item><description>VersionMajor (<see cref="int"/>)</description></item>
    /// <item><description>VersionMinor (<see cref="int"/>)</description></item>
    /// <item><description>VersionBuild (nullable <see cref="int"/>)</description></item>
    /// <item><description>VersionRevision (nullable <see cref="int"/>)</description></item>
    /// <item><description>Description (<see cref="string"/>)</description></item>
    /// <item><description>CommitDateUtc (<see cref="DateTime"/>)</description></item>
    /// <item><description>CommitDurationInTicks (<see cref="long"/>)</description></item>
    /// </list>
    /// </remarks>
    protected virtual void VersionHistoryTableBuilderInit(SqlTableBuilder builder)
    {
        var intType = builder.Database.TypeDefinitions.GetByType<int>();
        var columns = builder.Columns;
        columns.Create( SqlHelpers.VersionHistoryVersionMajorName ).SetType( intType );
        columns.Create( SqlHelpers.VersionHistoryVersionMinorName ).SetType( intType );
        columns.Create( SqlHelpers.VersionHistoryVersionBuildName ).SetType( intType ).MarkAsNullable();
        columns.Create( SqlHelpers.VersionHistoryVersionRevisionName ).SetType( intType ).MarkAsNullable();
        columns.Create( SqlHelpers.VersionHistoryDescriptionName ).SetType<string>();
        columns.Create( SqlHelpers.VersionHistoryCommitDateUtcName ).SetType<DateTime>();
        columns.Create( SqlHelpers.VersionHistoryCommitDurationInTicksName ).SetType<long>();
    }

    /// <summary>
    /// Creates a delegate capable of reading metadata of all versions applied to this database.
    /// </summary>
    /// <param name="queryReaders">Query reader factory.</param>
    /// <returns>Delegate capable of reading metadata of all versions applied to this database.</returns>
    [Pure]
    protected virtual Func<IDataReader, SqlQueryReaderOptions, SqlQueryResult<SqlDatabaseVersionRecord>>
        GetVersionHistoryRecordsQueryDelegate(SqlQueryReaderFactory queryReaders)
    {
        return static (reader, options) =>
        {
            var dbReader = ( DbDataReader )reader;
            if ( ! dbReader.Read() )
                return SqlQueryResult<SqlDatabaseVersionRecord>.Empty;

            var rows = options.CreateList<SqlDatabaseVersionRecord>();

            var iOrdinal = dbReader.GetOrdinal( SqlHelpers.VersionHistoryOrdinalName );
            var iVersionMajor = dbReader.GetOrdinal( SqlHelpers.VersionHistoryVersionMajorName );
            var iVersionMinor = dbReader.GetOrdinal( SqlHelpers.VersionHistoryVersionMinorName );
            var iVersionBuild = dbReader.GetOrdinal( SqlHelpers.VersionHistoryVersionBuildName );
            var iVersionRevision = dbReader.GetOrdinal( SqlHelpers.VersionHistoryVersionRevisionName );
            var iDescription = dbReader.GetOrdinal( SqlHelpers.VersionHistoryDescriptionName );
            var iCommitDateUtc = dbReader.GetOrdinal( SqlHelpers.VersionHistoryCommitDateUtcName );
            var iCommitDurationInTicks = dbReader.GetOrdinal( SqlHelpers.VersionHistoryCommitDurationInTicksName );

            do
            {
                var versionMajor = dbReader.GetInt32( iVersionMajor );
                var versionMinor = dbReader.GetInt32( iVersionMinor );

                var record = new SqlDatabaseVersionRecord(
                    Ordinal: dbReader.GetInt32( iOrdinal ),
                    Version: dbReader.IsDBNull( iVersionBuild )
                        ? new Version( versionMajor, versionMinor )
                        : dbReader.IsDBNull( iVersionRevision )
                            ? new Version( versionMajor, versionMinor, dbReader.GetInt32( iVersionBuild ) )
                            : new Version(
                                versionMajor,
                                versionMinor,
                                dbReader.GetInt32( iVersionBuild ),
                                dbReader.GetInt32( iVersionRevision ) ),
                    Description: dbReader.GetString( iDescription ),
                    CommitDateUtc: DateTime.SpecifyKind( dbReader.GetDateTime( iCommitDateUtc ), DateTimeKind.Utc ),
                    CommitDuration: TimeSpan.FromTicks( dbReader.GetInt64( iCommitDurationInTicks ) ) );

                rows.Add( record );
            }
            while ( dbReader.Read() );

            return new SqlQueryResult<SqlDatabaseVersionRecord>( resultSetFields: null, rows );
        };
    }

    /// <summary>
    /// Creates a new <see cref="SqlDatabaseCommitVersionsContext"/> instance, used for managing application of versions to the database.
    /// </summary>
    /// <param name="parameterBinders">Parameter binder factory.</param>
    /// <param name="options">DB creation options.</param>
    /// <returns>New <see cref="SqlDatabaseCommitVersionsContext"/> instance.</returns>
    [Pure]
    protected virtual SqlDatabaseCommitVersionsContext CreateCommitVersionsContext(
        SqlParameterBinderFactory parameterBinders,
        SqlCreateDatabaseOptions options)
    {
        return new SqlDatabaseCommitVersionsContext();
    }

    /// <summary>
    /// Allows to react to an unexpected exception.
    /// </summary>
    /// <param name="exception">Thrown exception.</param>
    /// <param name="connection">Opened connection to the database.</param>
    protected virtual void OnUncaughtException(Exception exception, DbConnection connection) { }

    private static void ApplyCommittedVersions(
        SqlDatabaseBuilder builder,
        SqlDatabaseConnectionStateChanges stateChanges,
        ReadOnlySpan<ISqlDatabaseVersion> versions)
    {
        Assume.Equals( builder.Changes.Mode, SqlDatabaseCreateMode.NoChanges );

        foreach ( var version in versions )
        {
            builder.Changes.Attach();
            version.Apply( builder );
            stateChanges.InvokePendingCallbacks();
        }
    }

    private static void ApplyUncommittedVersionsInDryRunMode(
        SqlDatabaseBuilder builder,
        ReadOnlySpan<ISqlDatabaseVersion> versions)
    {
        builder.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );

        foreach ( var version in versions )
        {
            builder.Changes.Attach();

            try
            {
                version.Apply( builder );
                builder.Changes.CompletePendingChanges();
            }
            finally
            {
                builder.Changes.ClearPendingActions();
            }
        }
    }

    private static (Exception? Exception, int AppliedVersions) ApplyUncommittedVersionsInCommitMode(
        SqlDatabaseBuilder builder,
        SqlDatabaseVersionHistory.DatabaseComparisonResult versions,
        SqlDatabaseConnectionStateChanges stateChanges,
        SqlDatabaseCommitVersionsContext context,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        builder.Changes.SetModeAndAttach( SqlDatabaseCreateMode.Commit );
        context.OnBeforeVersionRangeApplication( builder, connection, ref executor );

        using var command = connection.CreateCommand();
        var elapsedTimes = new List<SqlDatabaseVersionElapsedTime>();
        var nextVersionOrdinal = versions.NextOrdinal;
        Exception? exception = null;

        foreach ( var version in versions.Uncommitted )
        {
            builder.Changes.Attach();
            var stopwatch = StopwatchSlim.Create();

            version.Apply( builder );
            var actions = builder.Changes.GetPendingActions();
            stateChanges.InvokePendingCallbacks();
            var versionOrdinal = nextVersionOrdinal;
            var statementKey = SqlDatabaseFactoryStatementKey.Create( version.Value );

            try
            {
                statementKey = context.OnBeforeVersionTransaction( builder, statementKey, connection, ref executor );
                Assume.Equals( statementKey.Version, version.Value );

                try
                {
                    using var transaction = connection.BeginTransaction( IsolationLevel.Serializable );
                    command.Transaction = transaction;

                    statementKey = context.OnBeforeVersionActionRangeExecution( builder, statementKey, command, ref executor );
                    Assume.Equals( statementKey.Version, version.Value );
                    Assume.Equals( command.Transaction, transaction );

                    foreach ( var action in actions )
                    {
                        statementKey = statementKey.NextOrdinal();
                        executor.Execute( command, statementKey, SqlDatabaseFactoryStatementType.Change, action );
                    }

                    statementKey = context.OnAfterVersionActionRangeExecution( builder, statementKey, command, ref executor );
                    Assume.Equals( statementKey.Version, version.Value );
                    Assume.Equals( command.Transaction, transaction );

                    if ( context.PersistLastVersionHistoryRecordOnly )
                    {
                        statementKey = statementKey.NextOrdinal();
                        context.DeleteAllVersionHistoryRecords( transaction, statementKey, ref executor );
                    }

                    statementKey = statementKey.NextOrdinal();
                    context.InsertVersionHistoryRecord(
                        transaction,
                        versionOrdinal,
                        version.Value,
                        version.Description,
                        statementKey,
                        ref executor );

                    transaction.Commit();
                }
                finally
                {
                    command.Transaction = null;
                    context.OnAfterVersionTransaction( builder, statementKey, connection, ref executor );
                }

                var elapsedTime = stopwatch.ElapsedTime;
                if ( context.PersistLastVersionHistoryRecordOnly )
                    elapsedTimes.Clear();

                elapsedTimes.Add( new SqlDatabaseVersionElapsedTime( versionOrdinal, elapsedTime ) );
            }
            catch ( Exception exc )
            {
                exception = exc;
                break;
            }
            finally
            {
                builder.Changes.ClearPendingActions();
            }

            ++nextVersionOrdinal;
        }

        try
        {
            context.UpdateVersionHistoryRecords( CollectionsMarshal.AsSpan( elapsedTimes ), ref executor );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        context.OnAfterVersionRangeApplication( builder, connection, ref executor );

        return (exception, nextVersionOrdinal - versions.Committed.Length - 1);
    }

    [Pure]
    private static SqlDatabaseVersionHistory.DatabaseComparisonResult CompareVersionHistoryToDatabase(
        SqlDatabaseVersionHistory versionHistory,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        using var command = connection.CreateCommand();
        command.CommandText = versionRecordsQuery.Sql;
        var @delegate = versionRecordsQuery.Reader.Delegate;

        var registeredVersionRecords = executor.ExecuteForVersionHistory(
                command,
                cmd =>
                {
                    using var reader = cmd.ExecuteReader();
                    return @delegate( reader, default );
                } )
            .Rows;

        return versionHistory.CompareToDatabase( CollectionsMarshal.AsSpan( registeredVersionRecords ) );
    }

    private SqlTableBuilder InitializeVersionHistoryTable(
        SqlDatabaseBuilder builder,
        SqlSchemaObjectName name,
        SqlNodeInterpreter nodeInterpreter,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor)
    {
        builder.Changes.SetModeAndAttach( SqlDatabaseCreateMode.DryRun );
        var attach = GetChangeTrackerAttachmentForVersionHistoryTableInit(
            builder.Changes,
            name,
            nodeInterpreter,
            connection,
            ref executor );

        nodeInterpreter.Context.Clear();
        builder.Changes.Attach( attach );

        var table = builder.Schemas.Default.Objects.CreateTable( name.Object );
        VersionHistoryTableBuilderInit( table );
        var ordinal = table.Columns.Create( SqlHelpers.VersionHistoryOrdinalName ).SetType<int>();
        table.Constraints.SetPrimaryKey( ordinal.Asc() );

        Assume.ContainsAtLeast( table.Database.Schemas, 1 );
        Assume.Equals( table.Schema.Name, name.Schema );
        Assume.Equals( table.Name, name.Object );

        var actions = builder.Changes.GetPendingActions();
        if ( actions.Length == 0 )
            return table;

        using var transaction = connection.BeginTransaction( IsolationLevel.Serializable );
        using var command = transaction.CreateCommand();

        foreach ( var action in actions )
            executor.ExecuteForVersionHistory( command, action );

        transaction.Commit();
        builder.Changes.ClearPendingActions();
        return table;
    }

    [Pure]
    private SqlQueryReaderExecutor<SqlDatabaseVersionRecord> CreateVersionHistoryRecordsQuery(
        SqlTableBuilder versionHistoryTable,
        SqlNodeInterpreter nodeInterpreter)
    {
        var dataSource = versionHistoryTable.Node.ToDataSource();
        var query = dataSource.Select( dataSource.GetAll() );
        foreach ( var column in versionHistoryTable.Constraints.GetPrimaryKey().Index.Columns )
        {
            Assume.IsNotNull( column );
            query = query.OrderBy( column.Asc() );
        }

        nodeInterpreter.VisitDataSourceQuery( query );
        var sql = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        return new SqlQueryReader<SqlDatabaseVersionRecord>(
                versionHistoryTable.Database.Dialect,
                GetVersionHistoryRecordsQueryDelegate( versionHistoryTable.Database.QueryReaders ) )
            .Bind( sql );
    }

    [Pure]
    private static SqlQueryReaderExecutor<SqlDatabaseVersionRecord> GetVersionHistoryRecordsQueryForDatabaseComparison(
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> fullQuery,
        SqlDatabaseVersionHistoryMode queryMode,
        SqlTableBuilder versionHistoryTable,
        SqlNodeInterpreter nodeInterpreter)
    {
        if ( queryMode == SqlDatabaseVersionHistoryMode.AllRecords )
            return fullQuery;

        var dataSource = versionHistoryTable.Node.ToDataSource().Limit( SqlNode.Literal( 1 ) );
        var query = dataSource.Select( dataSource.GetAll() );
        foreach ( var column in versionHistoryTable.Constraints.GetPrimaryKey().Index.Columns )
        {
            Assume.IsNotNull( column );
            query = query.OrderBy( column.Desc() );
        }

        nodeInterpreter.VisitDataSourceQuery( query );
        var sql = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        return fullQuery.Reader.Bind( sql );
    }

    SqlCreateDatabaseResult<ISqlDatabase> ISqlDatabaseFactory.Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options)
    {
        return Create( connectionString, versionHistory, options );
    }
}
