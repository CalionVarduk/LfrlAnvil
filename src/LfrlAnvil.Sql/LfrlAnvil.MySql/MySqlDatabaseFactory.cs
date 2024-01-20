using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Extensions;
using LfrlAnvil.MySql.Extensions;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using MySqlConnector;

namespace LfrlAnvil.MySql;

public sealed class MySqlDatabaseFactory : ISqlDatabaseFactory
{
    public SqlDialect Dialect => MySqlDialect.Instance;

    public SqlCreateDatabaseResult<MySqlDatabase> Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options = default)
    {
        var connection = CreateConnection( connectionString );
        try
        {
            connection.Open();

            var connectionChangeEvent = new SqlDatabaseConnectionChangeEvent(
                connection,
                new StateChangeEventArgs( ConnectionState.Closed, ConnectionState.Open ) );

            var statementExecutor = new SqlStatementExecutor( options );
            var versionHistoryInfo = InitializeDatabaseBuilderWithVersionHistoryTable(
                connectionChangeEvent,
                options,
                ref statementExecutor );

            var builder = versionHistoryInfo.Table.Database;

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
            var database = new MySqlDatabase( connectionString, builder, versionRecordsQuery, newDbVersion );
            return new SqlCreateDatabaseResult<MySqlDatabase>( database, exception, versions, appliedVersionCount );
        }
        finally
        {
            connection.Dispose();
        }
    }

    [Pure]
    private static SqlDatabaseVersionHistory.DatabaseComparisonResult CompareVersionHistoryToDatabase(
        MySqlConnection connection,
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
    private MySqlConnection CreateConnection(string connectionString)
    {
        var builder = new MySqlConnectionStringBuilder( connectionString )
        {
            GuidFormat = MySqlGuidFormat.None,
            AllowUserVariables = true,
            NoBackslashEscapes = true,
            Database = string.Empty
        };

        return new MySqlConnection( builder.ToString() );
    }

    [Pure]
    private static MySqlDatabaseBuilder CreateBuilder(SqlDatabaseConnectionChangeEvent connectionChangeEvent, string commonSchemaName)
    {
        var result = new MySqlDatabaseBuilder( connectionChangeEvent.Connection.ServerVersion, commonSchemaName );
        result.AddConnectionChangeCallback( InitializeSessionSqlMode );
        InvokePendingConnectionChangeCallbacks( result, connectionChangeEvent );
        return result;
    }

    private static void SetBuilderMode(MySqlDatabaseBuilder builder, SqlDatabaseCreateMode mode)
    {
        builder.ChangeTracker.SetMode( mode );
    }

    private static void ClearBuilderStatements(MySqlDatabaseBuilder builder)
    {
        builder.ChangeTracker.ClearStatements();
    }

    [Pure]
    private static VersionHistoryInfo InitializeDatabaseBuilderWithVersionHistoryTable(
        SqlDatabaseConnectionChangeEvent connectionChangeEvent,
        SqlCreateDatabaseOptions options,
        ref SqlStatementExecutor statementExecutor)
    {
        var schemaName = options.VersionHistoryName?.Schema ?? "common";
        var tableName = options.VersionHistoryName?.Object ?? "__VersionHistory";

        var builder = CreateBuilder( connectionChangeEvent, schemaName );
        SetBuilderMode( builder, SqlDatabaseCreateMode.Commit );
        var interpreter = builder.NodeInterpreters.Create( SqlNodeInterpreterContext.Create( capacity: 256 ) );

        using ( var command = ReinterpretCast.To<MySqlConnection>( connectionChangeEvent.Connection ).CreateCommand() )
        {
            var schemata = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "information_schema", "schemata" ) );
            var query = SqlNode.DummyDataSource()
                .Select(
                    schemata.ToDataSource()
                        .AndWhere(
                            schemata.GetRawField( "schema_name", TypeNullability.Create<string>() ) == SqlNode.Literal( schemaName ) )
                        .Exists()
                        .ToValue()
                        .As( "x" ) );

            interpreter.VisitDataSourceQuery( query );

            command.CommandText = interpreter.Context.Sql.AppendSemicolon().ToString();
            interpreter.Context.Clear();

            var exists = statementExecutor.ExecuteVersionHistoryQuery( command, static cmd => Convert.ToBoolean( cmd.ExecuteScalar() ) );
            if ( ! exists )
            {
                builder.ChangeTracker.SchemaCreated( schemaName );
                builder.ChangeTracker.CreateGuidFunction();
                builder.ChangeTracker.CreateDropIndexIfExistsProcedure();
            }
            else
            {
                var routines = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "information_schema", "routines" ) );
                var routinesDataSource = routines.ToDataSource()
                    .AndWhere(
                        routines.GetRawField( "routine_schema", TypeNullability.Create<string>() ) == SqlNode.Literal( schemaName ) );

                var routineName = routines.GetRawField( "routine_name", TypeNullability.Create<string>() );
                var routineType = routines.GetRawField( "routine_type", TypeNullability.Create<string>() );

                query = SqlNode.DummyDataSource()
                    .Select(
                        routinesDataSource
                            .AndWhere( routineType == SqlNode.Literal( "FUNCTION" ) )
                            .AndWhere( routineName == SqlNode.Literal( "GUID" ) )
                            .Exists()
                            .ToValue()
                            .As( "x" ) );

                interpreter.VisitDataSourceQuery( query );

                command.CommandText = interpreter.Context.Sql.AppendSemicolon().ToString();
                interpreter.Context.Clear();

                exists = statementExecutor.ExecuteVersionHistoryQuery( command, static cmd => Convert.ToBoolean( cmd.ExecuteScalar() ) );
                if ( ! exists )
                    builder.ChangeTracker.CreateGuidFunction();

                query = SqlNode.DummyDataSource()
                    .Select(
                        routinesDataSource
                            .AndWhere( routineType == SqlNode.Literal( "PROCEDURE" ) )
                            .AndWhere( routineName == SqlNode.Literal( "_DROP_INDEX_IF_EXISTS" ) )
                            .Exists()
                            .ToValue()
                            .As( "x" ) );

                interpreter.VisitDataSourceQuery( query );

                command.CommandText = interpreter.Context.Sql.AppendSemicolon().ToString();
                interpreter.Context.Clear();

                exists = statementExecutor.ExecuteVersionHistoryQuery( command, static cmd => Convert.ToBoolean( cmd.ExecuteScalar() ) );
                if ( ! exists )
                    builder.ChangeTracker.CreateDropIndexIfExistsProcedure();
            }
        }

        var intType = builder.TypeDefinitions.GetByType<int>();
        var longType = builder.TypeDefinitions.GetByType<long>();
        var stringType = builder.TypeDefinitions.GetByType<string>();
        var dateTimeType =
            (MySqlColumnTypeDefinition<string>)builder.TypeDefinitions.GetByDataType( builder.DataTypes.GetFixedString( length: 27 ) );

        var table = builder.Schemas.Default.Objects.CreateTable( tableName );
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
            new VersionHistoryColumn<string>( commitDateUtc.Node, dateTimeType ),
            new VersionHistoryColumn<long>( commitDurationInTicks.Node, longType ),
            interpreter );
    }

    private static void CreateVersionHistoryTableInDatabaseIfNotExists(
        MySqlConnection connection,
        in VersionHistoryInfo info,
        ref SqlStatementExecutor statementExecutor)
    {
        var tables = SqlNode.RawRecordSet( SqlRecordSetInfo.Create( "information_schema", "tables" ) );
        var tableSchema = tables.GetRawField( "table_schema", TypeNullability.Create<string>() );
        var tableName = tables.GetRawField( "table_name", TypeNullability.Create<string>() );

        var query = SqlNode.DummyDataSource()
            .Select(
                tables.ToDataSource()
                    .AndWhere( tableSchema == SqlNode.Literal( info.Table.Schema.Name ) )
                    .AndWhere( tableName == SqlNode.Literal( info.Table.Name ) )
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
                        statement.Apply( command );
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
    private static MySqlTransaction CreateTransaction(MySqlCommand command)
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

        return new SqlQueryReader<SqlDatabaseVersionRecord>( MySqlDialect.Instance, Executor ).Bind( sql );

        [Pure]
        static SqlQueryReaderResult<SqlDatabaseVersionRecord> Executor(IDataReader reader, SqlQueryReaderOptions options)
        {
            var mySqlReader = (MySqlDataReader)reader;
            if ( ! mySqlReader.Read() )
                return SqlQueryReaderResult<SqlDatabaseVersionRecord>.Empty;

            var rows = options.InitialBufferCapacity is not null
                ? new List<SqlDatabaseVersionRecord>( capacity: options.InitialBufferCapacity.Value )
                : new List<SqlDatabaseVersionRecord>();

            var iOrdinal = mySqlReader.GetOrdinal( VersionHistoryInfo.OrdinalName );
            var iVersionMajor = mySqlReader.GetOrdinal( VersionHistoryInfo.VersionMajorName );
            var iVersionMinor = mySqlReader.GetOrdinal( VersionHistoryInfo.VersionMinorName );
            var iVersionBuild = mySqlReader.GetOrdinal( VersionHistoryInfo.VersionBuildName );
            var iVersionRevision = mySqlReader.GetOrdinal( VersionHistoryInfo.VersionRevisionName );
            var iDescription = mySqlReader.GetOrdinal( VersionHistoryInfo.DescriptionName );
            var iCommitDateUtc = mySqlReader.GetOrdinal( VersionHistoryInfo.CommitDateUtcName );
            var iCommitDurationInTicks = mySqlReader.GetOrdinal( VersionHistoryInfo.CommitDurationInTicksName );

            do
            {
                var versionMajor = mySqlReader.GetInt32( iVersionMajor );
                var versionMinor = mySqlReader.GetInt32( iVersionMinor );
                var versionBuild = mySqlReader.IsDBNull( iVersionBuild ) ? (int?)null : mySqlReader.GetInt32( iVersionBuild );
                var versionRevision = versionBuild is null || mySqlReader.IsDBNull( iVersionRevision )
                    ? (int?)null
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

            return new SqlQueryReaderResult<SqlDatabaseVersionRecord>( resultSetFields: null, rows );
        }
    }

    private static (Exception? Exception, int AppliedVersions) ApplyVersionsInDryRunMode(
        SqlDatabaseConnectionChangeEvent connectionChangeEvent,
        MySqlDatabaseBuilder builder,
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

        var connection = ReinterpretCast.To<MySqlConnection>( connectionChangeEvent.Connection );

        using var statementCommand = connection.CreateCommand();
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
            : OptionalDisposable<MySqlCommand>.Empty;

        var builder = versionHistory.Table.Database;
        var elapsedTimes = new List<(int Ordinal, TimeSpan ElapsedTime)>();
        var nextVersionOrdinal = versions.NextOrdinal;
        Exception? exception = null;

        foreach ( var version in versions.Uncommitted )
        {
            builder.SetAttachedMode();
            var start = Stopwatch.GetTimestamp();

            version.Apply( builder );
            var statements = builder.GetPendingStatements();
            InvokePendingConnectionChangeCallbacks( builder, connectionChangeEvent );
            var versionOrdinal = nextVersionOrdinal;
            var statementKey = SqlDatabaseFactoryStatementKey.Create( version.Value );

            try
            {
                try
                {
                    using var transaction = CreateTransaction( statementCommand );
                    insertVersionCommand.Transaction = transaction;

                    foreach ( var statement in statements )
                    {
                        statementKey = statementKey.NextOrdinal();
                        statement.Apply( statementCommand );
                        statementExecutor.ExecuteNonQuery( statementCommand, statementKey, SqlDatabaseFactoryStatementType.Change );
                    }

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
                    pCommitDateUtc.Value = versionHistory.CommitDateUtc.Type.ToParameterValue(
                        DateTime.UtcNow.ToString( "yyyy-MM-dd HH:mm:ss.fffffff", CultureInfo.InvariantCulture ) );

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

    private static void InvokePendingConnectionChangeCallbacks(MySqlDatabaseBuilder builder, SqlDatabaseConnectionChangeEvent @event)
    {
        var callbacks = builder.GetPendingConnectionChangeCallbacks();
        foreach ( var callback in callbacks )
            callback( @event );
    }

    [Pure]
    private static MySqlCommand PrepareInsertVersionRecordCommand(MySqlConnection connection, in VersionHistoryInfo info)
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
    private static MySqlCommand PrepareDeleteVersionRecordsCommand(MySqlConnection connection, in VersionHistoryInfo info)
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
        MySqlConnection connection,
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
    private static MySqlCommand PrepareUpdateVersionRecordCommitDurationCommand(MySqlConnection connection, in VersionHistoryInfo info)
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
    private static void AddCommandParameter<T>(MySqlCommand command, VersionHistoryColumn<T> column)
        where T : notnull
    {
        var parameter = command.CreateParameter();
        column.Type.SetParameterInfo( parameter, column.Node.Type.IsNullable );
        parameter.ParameterName = column.Node.Name;
        command.Parameters.Add( parameter );
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

    private readonly record struct VersionHistoryColumn<T>(SqlColumnBuilderNode Node, MySqlColumnTypeDefinition<T> Type)
        where T : notnull;

    private readonly record struct VersionHistoryInfo(
        MySqlTableBuilder Table,
        VersionHistoryColumn<int> Ordinal,
        VersionHistoryColumn<int> VersionMajor,
        VersionHistoryColumn<int> VersionMinor,
        VersionHistoryColumn<int> VersionBuild,
        VersionHistoryColumn<int> VersionRevision,
        VersionHistoryColumn<string> Description,
        VersionHistoryColumn<string> CommitDateUtc,
        VersionHistoryColumn<long> CommitDurationInTicks,
        MySqlNodeInterpreter Interpreter)
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

        internal void ExecuteVersionHistoryNonQuery(MySqlCommand command)
        {
            ExecuteNonQuery( command, VersionHistoryKey, SqlDatabaseFactoryStatementType.VersionHistory );
            VersionHistoryKey = VersionHistoryKey.NextOrdinal();
        }

        internal void ExecuteNonQuery(MySqlCommand command, SqlDatabaseFactoryStatementKey key, SqlDatabaseFactoryStatementType type)
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
            MySqlCommand command,
            Func<MySqlCommand, T> resultSelector,
            SqlDatabaseFactoryStatementType type = SqlDatabaseFactoryStatementType.VersionHistory)
        {
            var result = ExecuteQuery( command, VersionHistoryKey, type, resultSelector );
            VersionHistoryKey = VersionHistoryKey.NextOrdinal();
            return result;
        }

        internal T ExecuteQuery<T>(
            MySqlCommand command,
            SqlDatabaseFactoryStatementKey key,
            SqlDatabaseFactoryStatementType type,
            Func<MySqlCommand, T> resultSelector)
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
