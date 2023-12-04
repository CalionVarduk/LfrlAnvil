using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Exceptions;
using LfrlAnvil.Sqlite.Extensions;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;
using SqliteConnection = LfrlAnvil.Sqlite.Internal.SqliteConnection;

namespace LfrlAnvil.Sqlite;

public sealed class SqliteDatabaseFactory : ISqlDatabaseFactory
{
    public SqliteDatabaseFactory(bool isConnectionPermanent = false)
    {
        IsConnectionPermanent = isConnectionPermanent;
    }

    public SqlDialect Dialect => SqliteDialect.Instance;
    public bool IsConnectionPermanent { get; }

    public SqlCreateDatabaseResult<SqliteDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default)
    {
        IReadOnlyCollection<Action<SqlDatabaseConnectionChangeEvent>>? connectionChangeCallbacks = null;
        var connection = CreateConnection( connectionString );
        try
        {
            connection.Open();

            var connectionChangeEvent = new SqlDatabaseConnectionChangeEvent(
                connection,
                new StateChangeEventArgs( ConnectionState.Closed, ConnectionState.Open ) );

            var versionHistoryInfo = InitializeDatabaseBuilderWithVersionHistoryTable( connectionChangeEvent, options );
            var builder = versionHistoryInfo.Table.Database;
            connectionChangeCallbacks = builder.ConnectionChanges.Callbacks;

            var statementExecutor = new SqlStatementExecutor( options );
            CreateVersionHistoryTableInDatabaseIfNotExists( connection, in versionHistoryInfo, ref statementExecutor );

            var versionRecordsQuery = CreateVersionRecordsQuery( in versionHistoryInfo );
            var versions = CompareVersionHistoryToDatabase( connection, versionHistory, versionRecordsQuery, ref statementExecutor );

            foreach ( var version in versions.Committed )
            {
                builder.SetAttachedMode();
                version.Apply( builder );
            }

            SetBuilderMode( builder, options.Mode );

            var (exception, appliedVersionCount) = options.Mode switch
            {
                SqlDatabaseCreateMode.DryRun => ApplyVersionsInDryRunMode( connectionChangeEvent, builder, versions ),
                SqlDatabaseCreateMode.Commit => ApplyVersionsInCommitMode(
                    connectionChangeEvent,
                    versions,
                    options.VersionHistoryPersistenceMode,
                    in versionHistoryInfo,
                    ref statementExecutor ),
                _ => (null, 0)
            };

            var newDbVersion = appliedVersionCount > 0 ? versions.Uncommitted[appliedVersionCount - 1].Value : versions.Current;
            connection.ChangeCallbacks = connectionChangeCallbacks.ToArray();

            SqliteDatabase database = connection is SqlitePermanentConnection permanentConnection
                ? new SqlitePermanentlyConnectedDatabase( permanentConnection, builder, versionRecordsQuery, newDbVersion )
                : new SqlitePersistentDatabase( connectionString, builder, versionRecordsQuery, newDbVersion );

            return new SqlCreateDatabaseResult<SqliteDatabase>( database, exception, versions, appliedVersionCount );
        }
        catch
        {
            if ( connectionChangeCallbacks is not null )
                connection.ChangeCallbacks = connectionChangeCallbacks.ToArray();

            if ( connection is SqlitePermanentConnection )
                connection.Close();

            throw;
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Pure]
    private static SqlDatabaseVersionHistory.DatabaseComparisonResult CompareVersionHistoryToDatabase(
        SqliteConnection connection,
        SqlDatabaseVersionHistory versionHistory,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        ref SqlStatementExecutor statementExecutor)
    {
        using var command = connection.CreateCommand();
        command.CommandText = versionRecordsQuery.Sql;
        var @delegate = versionRecordsQuery.Reader.Delegate;

        var registeredVersionRecords = statementExecutor.ExecuteVersionHistoryQuery(
                command,
                cmd =>
                {
                    using var reader = cmd.ExecuteReader();
                    return @delegate( reader, default );
                } )
            .Rows;

        return versionHistory.CompareToDatabase( CollectionsMarshal.AsSpan( registeredVersionRecords ) );
    }

    [Pure]
    private SqliteConnection CreateConnection(string connectionString)
    {
        var cs = new SqliteConnectionStringBuilder( connectionString );
        return IsConnectionPermanent || cs.DataSource == ":memory:"
            ? new SqlitePermanentConnection( cs.ToString() )
            : new SqliteConnection( cs.ToString() );
    }

    [Pure]
    private static SqliteDatabaseBuilder CreateBuilder(SqlDatabaseConnectionChangeEvent connectionChangeEvent)
    {
        var result = new SqliteDatabaseBuilder( connectionChangeEvent.Connection.ServerVersion );
        result.AddConnectionChangeCallback( FunctionInitializer );
        InvokePendingConnectionChangeCallbacks( result, connectionChangeEvent );
        return result;
    }

    private static void SetBuilderMode(SqliteDatabaseBuilder builder, SqlDatabaseCreateMode mode)
    {
        builder.ChangeTracker.SetMode( mode );
    }

    private static void ClearBuilderStatements(SqliteDatabaseBuilder builder)
    {
        builder.ChangeTracker.ClearStatements();
    }

    [Pure]
    private static VersionHistoryInfo InitializeDatabaseBuilderWithVersionHistoryTable(
        SqlDatabaseConnectionChangeEvent connectionChangeEvent,
        SqlCreateDatabaseOptions options)
    {
        var builder = CreateBuilder( connectionChangeEvent );
        SetBuilderMode( builder, SqlDatabaseCreateMode.Commit );

        var intType = builder.TypeDefinitions.GetByType<int>();
        var longType = builder.TypeDefinitions.GetByType<long>();
        var stringType = builder.TypeDefinitions.GetByType<string>();
        var dateTimeType = builder.TypeDefinitions.GetByType<DateTime>();

        var schemaName = options.VersionHistorySchemaName ?? string.Empty;
        var tableName = options.VersionHistoryTableName ?? "__VersionHistory";

        var table = builder.Schemas.GetOrCreate( schemaName ).Objects.CreateTable( tableName );
        var columns = table.Columns;

        var ordinal = columns.Create( VersionHistoryInfo.OrdinalName ).SetType( intType );
        var versionMajor = columns.Create( VersionHistoryInfo.VersionMajorName ).SetType( intType );
        var versionMinor = columns.Create( VersionHistoryInfo.VersionMinorName ).SetType( intType );
        var versionBuild = columns.Create( VersionHistoryInfo.VersionBuildName ).SetType( intType ).MarkAsNullable();
        var versionRevision = columns.Create( VersionHistoryInfo.VersionRevisionName ).SetType( intType ).MarkAsNullable();
        var description = columns.Create( VersionHistoryInfo.DescriptionName ).SetType( stringType );
        var commitDateUtc = columns.Create( VersionHistoryInfo.CommitDateUtcName ).SetType( dateTimeType );
        var commitDurationInTicks = columns.Create( VersionHistoryInfo.CommitDurationInTicksName ).SetType( longType );
        table.SetPrimaryKey( ordinal.Asc() );

        return new VersionHistoryInfo(
            table,
            new VersionHistoryColumn<int>( ordinal.Node, intType ),
            new VersionHistoryColumn<int>( versionMajor.Node, intType ),
            new VersionHistoryColumn<int>( versionMinor.Node, intType ),
            new VersionHistoryColumn<int>( versionBuild.Node, intType ),
            new VersionHistoryColumn<int>( versionRevision.Node, intType ),
            new VersionHistoryColumn<string>( description.Node, stringType ),
            new VersionHistoryColumn<DateTime>( commitDateUtc.Node, dateTimeType ),
            new VersionHistoryColumn<long>( commitDurationInTicks.Node, longType ),
            builder.NodeInterpreters.Create( SqlNodeInterpreterContext.Create( capacity: 256 ) ) );
    }

    private static void CreateVersionHistoryTableInDatabaseIfNotExists(
        SqliteConnection connection,
        in VersionHistoryInfo info,
        ref SqlStatementExecutor statementExecutor)
    {
        var master = SqlNode.RawRecordSet( "\"sqlite_master\"" );
        var masterType = master.GetRawField( "type", TypeNullability.Create<string>() );
        var masterName = master.GetRawField( "name", TypeNullability.Create<string>() );

        var query = SqlNode.DummyDataSource()
            .Select(
                master.ToDataSource()
                    .AndWhere( masterType == SqlNode.Literal( "table" ) )
                    .AndWhere( masterName == SqlNode.Literal( info.Table.FullName ) )
                    .Exists()
                    .ToValue()
                    .As( "x" ) );

        info.Interpreter.VisitDataSourceQuery( query );

        using ( var command = connection.CreateCommand() )
        {
            command.CommandText = info.Interpreter.Context.Sql.AppendSemicolon().ToString();
            info.Interpreter.Context.Clear();

            var exists = statementExecutor.ExecuteVersionHistoryQuery( command, static cmd => Convert.ToBoolean( cmd.ExecuteScalar() ) );

            if ( ! exists )
            {
                using ( var transaction = CreateTransaction( command ) )
                {
                    var statements = info.Table.Database.GetPendingStatements();
                    foreach ( var statement in statements )
                    {
                        command.CommandText = statement.Sql;
                        statement.BeforeCallback?.Invoke( command );
                        statementExecutor.ExecuteVersionHistoryNonQuery( command );
                    }

                    transaction.Commit();
                }

                command.Transaction = null;
            }
        }

        SetBuilderMode( info.Table.Database, SqlDatabaseCreateMode.NoChanges );

        info.Table.Remove();
        if ( info.Table.Schema.CanRemove )
            info.Table.Schema.Remove();

        ClearBuilderStatements( info.Table.Database );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqliteTransaction CreateTransaction(SqliteCommand command)
    {
        Assume.IsNotNull( command.Connection );
        var result = command.Connection.BeginTransaction( IsolationLevel.Serializable );
        command.Transaction = result;
        return result;
    }

    [Pure]
    private static SqlQueryReaderExecutor<SqlDatabaseVersionRecord> CreateVersionRecordsQuery(in VersionHistoryInfo info)
    {
        var dataSource = info.Table.RecordSet.ToDataSource();
        var query = dataSource.Select( dataSource.GetAll() ).OrderBy( info.Ordinal.Node.Asc() );
        info.Interpreter.VisitDataSourceQuery( query );

        var sql = info.Interpreter.Context.Sql.AppendSemicolon().ToString();
        info.Interpreter.Context.Clear();

        return new SqlQueryReader<SqlDatabaseVersionRecord>( SqliteDialect.Instance, Executor ).Bind( sql );

        [Pure]
        static SqlQueryReaderResult<SqlDatabaseVersionRecord> Executor(IDataReader reader, SqlQueryReaderOptions options)
        {
            var sqliteReader = (SqliteDataReader)reader;
            if ( ! sqliteReader.Read() )
                return SqlQueryReaderResult<SqlDatabaseVersionRecord>.Empty;

            var rows = options.InitialBufferCapacity is not null
                ? new List<SqlDatabaseVersionRecord>( capacity: options.InitialBufferCapacity.Value )
                : new List<SqlDatabaseVersionRecord>();

            var iOrdinal = sqliteReader.GetOrdinal( VersionHistoryInfo.OrdinalName );
            var iVersionMajor = sqliteReader.GetOrdinal( VersionHistoryInfo.VersionMajorName );
            var iVersionMinor = sqliteReader.GetOrdinal( VersionHistoryInfo.VersionMinorName );
            var iVersionBuild = sqliteReader.GetOrdinal( VersionHistoryInfo.VersionBuildName );
            var iVersionRevision = sqliteReader.GetOrdinal( VersionHistoryInfo.VersionRevisionName );
            var iDescription = sqliteReader.GetOrdinal( VersionHistoryInfo.DescriptionName );
            var iCommitDateUtc = sqliteReader.GetOrdinal( VersionHistoryInfo.CommitDateUtcName );
            var iCommitDurationInTicks = sqliteReader.GetOrdinal( VersionHistoryInfo.CommitDurationInTicksName );

            do
            {
                var versionMajor = sqliteReader.GetInt32( iVersionMajor );
                var versionMinor = sqliteReader.GetInt32( iVersionMinor );
                var versionBuild = sqliteReader.IsDBNull( iVersionBuild ) ? (int?)null : sqliteReader.GetInt32( iVersionBuild );
                var versionRevision = versionBuild is null || sqliteReader.IsDBNull( iVersionRevision )
                    ? (int?)null
                    : sqliteReader.GetInt32( iVersionRevision );

                var record = new SqlDatabaseVersionRecord(
                    Ordinal: sqliteReader.GetInt32( iOrdinal ),
                    Version: versionBuild is null
                        ? new Version( versionMajor, versionMinor )
                        : versionRevision is null
                            ? new Version( versionMajor, versionMinor, versionBuild.Value )
                            : new Version( versionMajor, versionMinor, versionBuild.Value, versionRevision.Value ),
                    Description: sqliteReader.GetString( iDescription ),
                    CommitDateUtc: DateTime.Parse( sqliteReader.GetString( iCommitDateUtc ) ),
                    CommitDuration: TimeSpan.FromTicks( sqliteReader.GetInt64( iCommitDurationInTicks ) ) );

                rows.Add( record );
            }
            while ( sqliteReader.Read() );

            return new SqlQueryReaderResult<SqlDatabaseVersionRecord>( resultSetFields: null, rows );
        }
    }

    private static (Exception? Exception, int AppliedVersions) ApplyVersionsInDryRunMode(
        SqlDatabaseConnectionChangeEvent connectionChangeEvent,
        SqliteDatabaseBuilder builder,
        SqlDatabaseVersionHistory.DatabaseComparisonResult versions)
    {
        foreach ( var version in versions.Uncommitted )
        {
            builder.SetAttachedMode();

            try
            {
                version.Apply( builder );
                _ = builder.GetPendingStatements();
                InvokePendingConnectionChangeCallbacks( builder, connectionChangeEvent );
            }
            finally
            {
                ClearBuilderStatements( builder );
            }
        }

        return (null, 0);
    }

    private static (Exception? Exception, int AppliedVersions) ApplyVersionsInCommitMode(
        SqlDatabaseConnectionChangeEvent connectionChangeEvent,
        SqlDatabaseVersionHistory.DatabaseComparisonResult versions,
        SqlDatabaseVersionHistoryPersistenceMode versionHistoryPersistenceMode,
        in VersionHistoryInfo versionHistory,
        ref SqlStatementExecutor statementExecutor)
    {
        if ( versions.Uncommitted.Length == 0 )
            return (null, 0);

        var connection = ReinterpretCast.To<SqliteConnection>( connectionChangeEvent.Connection );

        using var statementCommand = connection.CreateCommand();
        var (preparePragmaText, restorePragmaText) = CreatePragmaCommandTexts( statementCommand, ref statementExecutor );

        using var insertVersionCommand = PrepareInsertVersionRecordCommand( connection, in versionHistory );
        var pOrdinal = insertVersionCommand.Parameters[0];
        var pVersionMajor = insertVersionCommand.Parameters[1];
        var pVersionMinor = insertVersionCommand.Parameters[2];
        var pVersionBuild = insertVersionCommand.Parameters[3];
        var pVersionRevision = insertVersionCommand.Parameters[4];
        var pDescription = insertVersionCommand.Parameters[5];
        var pCommitDateUtc = insertVersionCommand.Parameters[6];

        using var deleteVersionsCommand = versionHistoryPersistenceMode == SqlDatabaseVersionHistoryPersistenceMode.LastRecordOnly
            ? OptionalDisposable.Create( PrepareDeleteVersionRecordsCommand( connection, in versionHistory ) )
            : OptionalDisposable<SqliteCommand>.Empty;

        var builder = versionHistory.Table.Database;
        var elapsedTimes = new List<(int Ordinal, TimeSpan ElapsedTime)>();
        var fkCheckFailures = new HashSet<string>();
        var nextVersionOrdinal = versions.NextOrdinal;
        Exception? exception = null;

        foreach ( var version in versions.Uncommitted )
        {
            fkCheckFailures.Clear();
            builder.SetAttachedMode();
            var start = Stopwatch.GetTimestamp();

            version.Apply( builder );
            var statements = builder.GetPendingStatements();
            InvokePendingConnectionChangeCallbacks( builder, connectionChangeEvent );
            var versionOrdinal = nextVersionOrdinal;
            var pragmaSwapped = false;
            var statementKey = SqlDatabaseFactoryStatementKey.Create( version.Value );

            try
            {
                if ( preparePragmaText.Length > 0 )
                {
                    statementKey = statementKey.NextOrdinal();
                    statementCommand.CommandText = preparePragmaText;
                    statementExecutor.ExecuteNonQuery( statementCommand, statementKey, SqlDatabaseFactoryStatementType.Other );
                    pragmaSwapped = true;
                }

                try
                {
                    using var transaction = CreateTransaction( statementCommand );
                    insertVersionCommand.Transaction = transaction;

                    foreach ( var statement in statements )
                    {
                        statementKey = statementKey.NextOrdinal();
                        statementCommand.CommandText = statement.Sql;
                        statement.BeforeCallback?.Invoke( statementCommand );
                        statementExecutor.ExecuteNonQuery( statementCommand, statementKey, SqlDatabaseFactoryStatementType.Change );
                    }

                    foreach ( var tableName in builder.ChangeTracker.ModifiedTableNames )
                    {
                        statementKey = statementKey.NextOrdinal();
                        statementCommand.CommandText = $"PRAGMA foreign_key_check('{tableName}');";

                        var hasFkFailure = statementExecutor.ExecuteQuery(
                            statementCommand,
                            statementKey,
                            SqlDatabaseFactoryStatementType.Other,
                            static cmd =>
                            {
                                using var reader = cmd.ExecuteReader();
                                return reader.Read();
                            } );

                        if ( hasFkFailure )
                            fkCheckFailures.Add( tableName );
                    }

                    // TODO: can use PRAGMA quick_check(TABLENAME) to run similar tests, but for CHECK constrains

                    if ( fkCheckFailures.Count > 0 )
                        throw new SqliteForeignKeyCheckException( version.Value, fkCheckFailures );

                    if ( deleteVersionsCommand.Value is not null )
                    {
                        statementKey = statementKey.NextOrdinal();
                        deleteVersionsCommand.Value.Transaction = transaction;
                        statementExecutor.ExecuteNonQuery(
                            deleteVersionsCommand.Value,
                            statementKey,
                            SqlDatabaseFactoryStatementType.VersionHistory );
                    }

                    pOrdinal.Value = versionHistory.Ordinal.Type.ToParameterValue( versionOrdinal );
                    pVersionMajor.Value = versionHistory.VersionMajor.Type.ToParameterValue( version.Value.Major );
                    pVersionMinor.Value = versionHistory.VersionMinor.Type.ToParameterValue( version.Value.Minor );

                    pVersionBuild.Value = version.Value.Build >= 0
                        ? versionHistory.VersionBuild.Type.ToParameterValue( version.Value.Build )
                        : DBNull.Value;

                    pVersionRevision.Value = version.Value.Revision >= 0
                        ? versionHistory.VersionRevision.Type.ToParameterValue( version.Value.Revision )
                        : DBNull.Value;

                    pDescription.Value = versionHistory.Description.Type.ToParameterValue( version.Description );
                    pCommitDateUtc.Value = versionHistory.CommitDateUtc.Type.ToParameterValue( DateTime.UtcNow );

                    statementKey = statementKey.NextOrdinal();
                    statementExecutor.ExecuteNonQuery( insertVersionCommand, statementKey, SqlDatabaseFactoryStatementType.VersionHistory );

                    transaction.Commit();
                }
                finally
                {
                    statementCommand.Transaction = null;
                    insertVersionCommand.Transaction = null;
                    if ( deleteVersionsCommand.Value is not null )
                        deleteVersionsCommand.Value.Transaction = null;

                    if ( pragmaSwapped && restorePragmaText.Length > 0 )
                    {
                        statementKey = statementKey.NextOrdinal();
                        statementCommand.CommandText = restorePragmaText;
                        statementExecutor.ExecuteNonQuery( statementCommand, statementKey, SqlDatabaseFactoryStatementType.Other );
                    }
                }

                var elapsedTime = StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() );

                if ( deleteVersionsCommand.Value is not null )
                    elapsedTimes.Clear();

                elapsedTimes.Add( (versionOrdinal, elapsedTime) );
            }
            catch ( Exception exc )
            {
                exception = exc;
                break;
            }
            finally
            {
                ClearBuilderStatements( builder );
            }

            ++nextVersionOrdinal;
        }

        try
        {
            UpdateVersionRecordRangeElapsedTime(
                connection,
                CollectionsMarshal.AsSpan( elapsedTimes ),
                in versionHistory,
                ref statementExecutor );
        }
        catch ( Exception exc )
        {
            exception = exc;
        }

        return (exception, nextVersionOrdinal - versions.Committed.Length - 1);
    }

    private static void InvokePendingConnectionChangeCallbacks(SqliteDatabaseBuilder builder, SqlDatabaseConnectionChangeEvent @event)
    {
        var callbacks = builder.GetPendingConnectionChangeCallbacks();
        foreach ( var callback in callbacks )
            callback( @event );
    }

    [Pure]
    private static (string Prepare, string Restore) CreatePragmaCommandTexts(
        SqliteCommand command,
        ref SqlStatementExecutor statementExecutor)
    {
        command.CommandText = "PRAGMA foreign_keys; PRAGMA legacy_alter_table;";
        var (areForeignKeysEnabled, isLegacyAlterTableEnabled) = statementExecutor.ExecuteVersionHistoryQuery(
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

        var prepareText = string.Empty;
        var restoreText = string.Empty;

        if ( areForeignKeysEnabled )
        {
            prepareText = "PRAGMA foreign_keys = 0;";
            restoreText = "PRAGMA foreign_keys = 1;";
        }

        if ( ! isLegacyAlterTableEnabled )
        {
            prepareText += "PRAGMA legacy_alter_table = 1;";
            restoreText += "PRAGMA legacy_alter_table = 0;";
        }

        return (prepareText, restoreText);
    }

    [Pure]
    private static SqliteCommand PrepareInsertVersionRecordCommand(SqliteConnection connection, in VersionHistoryInfo info)
    {
        var pOrdinal = SqlNode.Parameter( VersionHistoryInfo.OrdinalName, info.Ordinal.Node.Type );
        var pVersionMajor = SqlNode.Parameter( VersionHistoryInfo.VersionMajorName, info.VersionMajor.Node.Type );
        var pVersionMinor = SqlNode.Parameter( VersionHistoryInfo.VersionMinorName, info.VersionMinor.Node.Type );
        var pVersionBuild = SqlNode.Parameter( VersionHistoryInfo.VersionBuildName, info.VersionBuild.Node.Type );
        var pVersionRevision = SqlNode.Parameter( VersionHistoryInfo.VersionRevisionName, info.VersionRevision.Node.Type );
        var pDescription = SqlNode.Parameter( VersionHistoryInfo.DescriptionName, info.Description.Node.Type );
        var pCommitDateUtc = SqlNode.Parameter( VersionHistoryInfo.CommitDateUtcName, info.CommitDateUtc.Node.Type );

        var insertInto = SqlNode.Values(
                pOrdinal,
                pVersionMajor,
                pVersionMinor,
                pVersionBuild,
                pVersionRevision,
                pDescription,
                pCommitDateUtc,
                SqlNode.Literal( 0 ) )
            .ToInsertInto(
                info.Table.RecordSet,
                info.Ordinal.Node,
                info.VersionMajor.Node,
                info.VersionMinor.Node,
                info.VersionBuild.Node,
                info.VersionRevision.Node,
                info.Description.Node,
                info.CommitDateUtc.Node,
                info.CommitDurationInTicks.Node );

        info.Interpreter.VisitInsertInto( insertInto );

        var command = connection.CreateCommand();
        try
        {
            command.CommandText = info.Interpreter.Context.Sql.AppendSemicolon().ToString();
            info.Interpreter.Context.Clear();

            AddCommandParameter( command, info.Ordinal );
            AddCommandParameter( command, info.VersionMajor );
            AddCommandParameter( command, info.VersionMinor );
            AddCommandParameter( command, info.VersionBuild );
            AddCommandParameter( command, info.VersionRevision );
            AddCommandParameter( command, info.Description );
            AddCommandParameter( command, info.CommitDateUtc );
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }

        return command;
    }

    [Pure]
    private static SqliteCommand PrepareDeleteVersionRecordsCommand(SqliteConnection connection, in VersionHistoryInfo info)
    {
        var deleteFrom = info.Table.RecordSet.ToDataSource().ToDeleteFrom();
        info.Interpreter.VisitDeleteFrom( deleteFrom );

        var command = connection.CreateCommand();
        try
        {
            command.CommandText = info.Interpreter.Context.Sql.AppendSemicolon().ToString();
            info.Interpreter.Context.Clear();
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }

        return command;
    }

    private static void UpdateVersionRecordRangeElapsedTime(
        SqliteConnection connection,
        ReadOnlySpan<(int Ordinal, TimeSpan ElapsedTime)> elapsedTimes,
        in VersionHistoryInfo info,
        ref SqlStatementExecutor statementExecutor)
    {
        if ( elapsedTimes.Length == 0 )
            return;

        using var command = PrepareUpdateVersionRecordCommitDurationCommand( connection, in info );
        var pCommitDurationInTicks = command.Parameters[0];
        var pOrdinal = command.Parameters[1];

        using var transaction = CreateTransaction( command );

        foreach ( var (ordinal, elapsedTime) in elapsedTimes )
        {
            pCommitDurationInTicks.Value = info.CommitDurationInTicks.Type.ToParameterValue( elapsedTime.Ticks );
            pOrdinal.Value = info.Ordinal.Type.ToParameterValue( ordinal );
            statementExecutor.ExecuteVersionHistoryNonQuery( command );
        }

        transaction.Commit();
    }

    [Pure]
    private static SqliteCommand PrepareUpdateVersionRecordCommitDurationCommand(SqliteConnection connection, in VersionHistoryInfo info)
    {
        var pCommitDuration = SqlNode.Parameter( VersionHistoryInfo.CommitDurationInTicksName, info.CommitDurationInTicks.Node.Type );
        var pOrdinal = SqlNode.Parameter( VersionHistoryInfo.OrdinalName, info.Ordinal.Node.Type );

        var update = info.Table.RecordSet
            .ToDataSource()
            .AndWhere( info.Ordinal.Node == pOrdinal )
            .ToUpdate( info.CommitDurationInTicks.Node.Assign( pCommitDuration ) );

        info.Interpreter.VisitUpdate( update );

        var command = connection.CreateCommand();
        try
        {
            command.CommandText = info.Interpreter.Context.Sql.AppendSemicolon().ToString();
            info.Interpreter.Context.Clear();

            AddCommandParameter( command, info.CommitDurationInTicks );
            AddCommandParameter( command, info.Ordinal );
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }

        return command;
    }

    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static void AddCommandParameter<T>(SqliteCommand command, VersionHistoryColumn<T> column)
        where T : notnull
    {
        var parameter = command.CreateParameter();
        column.Type.SetParameterInfo( parameter, column.Node.Type.IsNullable );
        parameter.ParameterName = column.Node.Name;
        command.Parameters.Add( parameter );
    }

    private static void FunctionInitializer(SqlDatabaseConnectionChangeEvent @event)
    {
        if ( @event.StateChange.CurrentState != ConnectionState.Open )
            return;

        var connection = ReinterpretCast.To<Microsoft.Data.Sqlite.SqliteConnection>( @event.Connection );

        using ( var command = connection.CreateCommand() )
        {
            command.CommandText = "PRAGMA foreign_keys = 1; PRAGMA ignore_check_constraints = 0;";
            command.ExecuteNonQuery();
        }

        connection.CreateFunction( "GET_CURRENT_DATE", GetCurrentDateImpl );
        connection.CreateFunction( "GET_CURRENT_TIME", GetCurrentTimeImpl );
        connection.CreateFunction( "GET_CURRENT_DATETIME", GetCurrentDateTimeImpl );
        connection.CreateFunction( "GET_CURRENT_TIMESTAMP", GetCurrentTimestampImpl );
        connection.CreateFunction( "NEW_GUID", NewGuidImpl );
        connection.CreateFunction<string?, string?>( "TO_LOWER", ToLowerImpl, isDeterministic: true );
        connection.CreateFunction<string?, string?>( "TO_UPPER", ToUpperImpl, isDeterministic: true );
        connection.CreateFunction<string?, string?, long?>( "INSTR_LAST", InstrLastImpl, isDeterministic: true );
    }

    [Pure]
    private static string GetCurrentDateImpl()
    {
        return DateTime.Now.ToString( "yyyy-MM-dd", CultureInfo.InvariantCulture );
    }

    [Pure]
    private static string GetCurrentTimeImpl()
    {
        return DateTime.Now.ToString( "HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }

    [Pure]
    private static string GetCurrentDateTimeImpl()
    {
        return DateTime.Now.ToString( "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture );
    }

    [Pure]
    private static long GetCurrentTimestampImpl()
    {
        return DateTime.UtcNow.Ticks;
    }

    [Pure]
    private static byte[] NewGuidImpl()
    {
        return Guid.NewGuid().ToByteArray();
    }

    [Pure]
    private static string? ToLowerImpl(string? s)
    {
        return s?.ToLowerInvariant();
    }

    [Pure]
    private static string? ToUpperImpl(string? s)
    {
        return s?.ToUpperInvariant();
    }

    [Pure]
    private static long? InstrLastImpl(string? s, string? v)
    {
        return s is not null && v is not null ? s.LastIndexOf( v, StringComparison.Ordinal ) + 1 : null;
    }

    private readonly record struct VersionHistoryColumn<T>(SqlColumnBuilderNode Node, SqliteColumnTypeDefinition<T> Type)
        where T : notnull;

    private readonly record struct VersionHistoryInfo(
        SqliteTableBuilder Table,
        VersionHistoryColumn<int> Ordinal,
        VersionHistoryColumn<int> VersionMajor,
        VersionHistoryColumn<int> VersionMinor,
        VersionHistoryColumn<int> VersionBuild,
        VersionHistoryColumn<int> VersionRevision,
        VersionHistoryColumn<string> Description,
        VersionHistoryColumn<DateTime> CommitDateUtc,
        VersionHistoryColumn<long> CommitDurationInTicks,
        SqliteNodeInterpreter Interpreter)
    {
        public const string OrdinalName = nameof( Ordinal );
        public const string VersionMajorName = nameof( VersionMajor );
        public const string VersionMinorName = nameof( VersionMinor );
        public const string VersionBuildName = nameof( VersionBuild );
        public const string VersionRevisionName = nameof( VersionRevision );
        public const string DescriptionName = nameof( Description );
        public const string CommitDateUtcName = nameof( CommitDateUtc );
        public const string CommitDurationInTicksName = nameof( CommitDurationInTicks );
    }

    private ref struct SqlStatementExecutor
    {
        internal SqlStatementExecutor(SqlCreateDatabaseOptions options)
        {
            VersionHistoryKey = SqlDatabaseFactoryStatementKey.Create( SqlDatabaseVersionHistory.InitialVersion ).NextOrdinal();
            Listeners = options.GetStatementListeners();
        }

        internal SqlDatabaseFactoryStatementKey VersionHistoryKey { get; private set; }
        internal ReadOnlySpan<ISqlDatabaseFactoryStatementListener> Listeners { get; }

        internal void ExecuteVersionHistoryNonQuery(SqliteCommand command)
        {
            ExecuteNonQuery( command, VersionHistoryKey, SqlDatabaseFactoryStatementType.VersionHistory );
            VersionHistoryKey = VersionHistoryKey.NextOrdinal();
        }

        internal void ExecuteNonQuery(SqliteCommand command, SqlDatabaseFactoryStatementKey key, SqlDatabaseFactoryStatementType type)
        {
            var @event = SqlDatabaseFactoryStatementEvent.Create( key, command, type );
            var start = Stopwatch.GetTimestamp();

            OnBeforeStatementExecution( @event );

            try
            {
                command.ExecuteNonQuery();
            }
            catch ( Exception exc )
            {
                OnAfterStatementExecution( @event, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), exc );
                throw;
            }

            OnAfterStatementExecution( @event, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), null );
        }

        internal T ExecuteVersionHistoryQuery<T>(
            SqliteCommand command,
            Func<SqliteCommand, T> resultSelector,
            SqlDatabaseFactoryStatementType type = SqlDatabaseFactoryStatementType.VersionHistory)
        {
            var result = ExecuteQuery( command, VersionHistoryKey, type, resultSelector );
            VersionHistoryKey = VersionHistoryKey.NextOrdinal();
            return result;
        }

        internal T ExecuteQuery<T>(
            SqliteCommand command,
            SqlDatabaseFactoryStatementKey key,
            SqlDatabaseFactoryStatementType type,
            Func<SqliteCommand, T> resultSelector)
        {
            T result;
            var @event = SqlDatabaseFactoryStatementEvent.Create( key, command, type );
            var start = Stopwatch.GetTimestamp();

            OnBeforeStatementExecution( @event );

            try
            {
                result = resultSelector( command );
            }
            catch ( Exception exc )
            {
                OnAfterStatementExecution( @event, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), exc );
                throw;
            }

            OnAfterStatementExecution( @event, StopwatchTimestamp.GetTimeSpan( start, Stopwatch.GetTimestamp() ), null );
            return result;
        }

        private void OnBeforeStatementExecution(SqlDatabaseFactoryStatementEvent @event)
        {
            foreach ( var listener in Listeners )
                listener.OnBefore( @event );
        }

        private void OnAfterStatementExecution(SqlDatabaseFactoryStatementEvent @event, TimeSpan elapsedTime, Exception? exception)
        {
            foreach ( var listener in Listeners )
                listener.OnAfter( @event, elapsedTime, exception );
        }
    }

    SqlCreateDatabaseResult<ISqlDatabase> ISqlDatabaseFactory.Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options)
    {
        return Create( connectionString, versionHistory, options );
    }
}
