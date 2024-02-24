using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
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

public abstract class SqlDatabaseFactory<TDatabase> : ISqlDatabaseFactory
    where TDatabase : SqlDatabase
{
    protected SqlDatabaseFactory(SqlDialect dialect)
    {
        Dialect = dialect;
    }

    public SqlDialect Dialect { get; }

    public SqlCreateDatabaseResult<TDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default)
    {
        var connectionStringBuilder = CreateConnectionStringBuilder( connectionString );
        var connection = CreateConnection( connectionStringBuilder );
        Assume.NotEquals( connection.State, ConnectionState.Open );
        var stateChanges = new SqlDatabaseConnectionStateChanges();

        try
        {
            connection.StateChange += stateChanges.Add;
            connection.Open();

            var executor = new SqlDatabaseFactoryStatementExecutor( options );
            var versionHistoryName = options.VersionHistoryName ?? GetDefaultVersionHistoryName();
            var builder = CreateDatabaseBuilder( versionHistoryName.Schema, connection, ref executor );
            Assume.Equals( builder.Schemas.Default.Name, versionHistoryName.Schema );
            Assume.IsEmpty( builder.Schemas.Default.Objects );
            Assume.ContainsExactly( builder.Schemas, 1 );

            var nodeInterpreter = builder.NodeInterpreters.Create( SqlNodeInterpreterContext.Create( capacity: 256 ) );
            var versionHistoryTable = InitializeVersionHistoryTable(
                builder,
                versionHistoryName,
                nodeInterpreter,
                connection,
                ref executor );

            builder.Changes.SetMode( SqlDatabaseCreateMode.NoChanges );
            stateChanges.SetBuilder( builder );

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
                    Assume.ContainsExactly( builder.Schemas, 1 );
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
                        database: CreateDatabase( builder, connectionStringBuilder, connection, versionHistoryRecordsQuery, newDbVersion ),
                        exception: exception,
                        versions: versions,
                        appliedVersionCount: appliedVersionCount );
                }
                case SqlDatabaseCreateMode.DryRun:
                {
                    versionHistoryTable.Remove();
                    Assume.ContainsExactly( builder.Schemas, 1 );
                    Assume.IsEmpty( builder.Schemas.Default.Objects );

                    ApplyCommittedVersions( builder, stateChanges, versions.Committed );
                    ApplyUncommittedVersionsInDryRunMode( builder, versions.Uncommitted );

                    return new SqlCreateDatabaseResult<TDatabase>(
                        database: CreateDatabase(
                            builder,
                            connectionStringBuilder,
                            connection,
                            versionHistoryRecordsQuery,
                            versions.Current ),
                        exception: null,
                        versions: versions,
                        appliedVersionCount: 0 );
                }
                default:
                {
                    versionHistoryTable.Remove();
                    Assume.ContainsExactly( builder.Schemas, 1 );
                    Assume.IsEmpty( builder.Schemas.Default.Objects );

                    ApplyCommittedVersions( builder, stateChanges, versions.Committed );

                    return new SqlCreateDatabaseResult<TDatabase>(
                        database: CreateDatabase(
                            builder,
                            connectionStringBuilder,
                            connection,
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
        }
    }

    [Pure]
    protected abstract DbConnectionStringBuilder CreateConnectionStringBuilder(string connectionString);

    [Pure]
    protected abstract DbConnection CreateConnection(DbConnectionStringBuilder connectionString);

    protected abstract SqlDatabaseBuilder CreateDatabaseBuilder(
        string defaultSchemaName,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor);

    protected abstract TDatabase CreateDatabase(
        SqlDatabaseBuilder builder,
        DbConnectionStringBuilder connectionString,
        DbConnection connection,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionHistoryRecordsQuery,
        Version version);

    [Pure]
    protected abstract SqlSchemaObjectName GetDefaultVersionHistoryName();

    protected abstract bool GetChangeTrackerAttachmentForVersionHistoryTableInit(
        SqlDatabaseChangeTracker changeTracker,
        SqlSchemaObjectName versionHistoryTableName,
        SqlNodeInterpreter nodeInterpreter,
        DbConnection connection,
        ref SqlDatabaseFactoryStatementExecutor executor);

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

    [Pure]
    protected virtual Func<IDataReader, SqlQueryReaderOptions, SqlQueryReaderResult<SqlDatabaseVersionRecord>>
        GetVersionHistoryRecordsQueryDelegate(SqlQueryReaderFactory queryReaders)
    {
        return static (reader, options) =>
        {
            var dbReader = (DbDataReader)reader;
            if ( ! dbReader.Read() )
                return SqlQueryReaderResult<SqlDatabaseVersionRecord>.Empty;

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

            return new SqlQueryReaderResult<SqlDatabaseVersionRecord>( resultSetFields: null, rows );
        };
    }

    [Pure]
    protected virtual SqlDatabaseCommitVersionsContext CreateCommitVersionsContext(
        SqlParameterBinderFactory parameterBinders,
        SqlCreateDatabaseOptions options)
    {
        return new SqlDatabaseCommitVersionsContext();
    }

    protected virtual void OnUncaughtException(Exception exception, DbConnection connection) { }

    private static void ApplyCommittedVersions(
        SqlDatabaseBuilder builder,
        SqlDatabaseConnectionStateChanges stateChanges,
        ReadOnlySpan<SqlDatabaseVersion> versions)
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
        ReadOnlySpan<SqlDatabaseVersion> versions)
    {
        builder.Changes.SetMode( SqlDatabaseCreateMode.DryRun );

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
        builder.Changes.SetMode( SqlDatabaseCreateMode.Commit );
        context.OnBeforeVersionRangeApplication( builder, connection, ref executor );

        using var command = connection.CreateCommand();
        var elapsedTimes = new List<SqlDatabaseVersionElapsedTime>();
        var nextVersionOrdinal = versions.NextOrdinal;
        Exception? exception = null;

        foreach ( var version in versions.Uncommitted )
        {
            builder.Changes.Attach();
            var start = Stopwatch.GetTimestamp();

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

                var elapsedTime = StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() );
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
        var attach = GetChangeTrackerAttachmentForVersionHistoryTableInit(
            builder.Changes,
            name,
            nodeInterpreter,
            connection,
            ref executor );

        nodeInterpreter.Context.Clear();
        builder.Changes.SetMode( SqlDatabaseCreateMode.DryRun );
        builder.Changes.Attach( attach );

        var table = builder.Schemas.Default.Objects.CreateTable( name.Object );
        VersionHistoryTableBuilderInit( table );
        var ordinal = table.Columns.Create( SqlHelpers.VersionHistoryOrdinalName ).SetType<int>();
        table.Constraints.SetPrimaryKey( ordinal.Asc() );

        Assume.ContainsExactly( table.Database.Schemas, 1 );
        Assume.Equals( table.Schema.Name, name.Schema );
        Assume.Equals( table.Name, name.Object );

        var actions = builder.Changes.GetPendingActions();
        if ( actions.Length == 0 )
            return table;

        using var transaction = connection.BeginTransaction( IsolationLevel.Serializable );
        using var command = connection.CreateCommand();

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
        foreach ( var c in versionHistoryTable.Constraints.GetPrimaryKey().Index.Columns )
            query = query.OrderBy( c.Column.Node.Asc() );

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
        foreach ( var c in versionHistoryTable.Constraints.GetPrimaryKey().Index.Columns )
            query = query.OrderBy( c.Column.Node.Desc() );

        nodeInterpreter.VisitDataSourceQuery( query );
        var sql = nodeInterpreter.Context.Sql.AppendSemicolon().ToString();
        nodeInterpreter.Context.Clear();

        return new SqlQueryReader<SqlDatabaseVersionRecord>( versionHistoryTable.Database.Dialect, fullQuery.Reader.Delegate ).Bind( sql );
    }

    SqlCreateDatabaseResult<ISqlDatabase> ISqlDatabaseFactory.Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options)
    {
        return Create( connectionString, versionHistory, options );
    }
}
