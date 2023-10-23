using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Events;
using LfrlAnvil.Sql.Objects.Builders;
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

            var (versionHistoryTable, types) = InitializeDatabaseBuilderWithVersionHistoryTable( connectionChangeEvent, options );
            var builder = versionHistoryTable.Database;
            connectionChangeCallbacks = builder.ConnectionChanges.Callbacks;

            var statementExecutor = new SqlStatementExecutor( options );
            CreateVersionHistoryTableInDatabaseIfNotExists( connection, versionHistoryTable, ref statementExecutor );

            var versionRecordsReader = CreateVersionRecordsReader( versionHistoryTable );
            var versions = CompareVersionHistoryToDatabase( connection, versionHistory, versionRecordsReader, ref statementExecutor );

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
                    versionHistoryTable,
                    versions,
                    types,
                    options.VersionHistoryPersistenceMode,
                    ref statementExecutor ),
                _ => (null, 0)
            };

            var appliedVersions = versions.Uncommitted.Slice( 0, appliedVersionCount );
            var pendingVersions = versions.Uncommitted.Slice( appliedVersions.Length );
            var newDbVersion = appliedVersions.Length > 0 ? appliedVersions[^1].Value : versions.Current;

            connection.ChangeCallbacks = connectionChangeCallbacks.ToArray();

            SqliteDatabase database = connection is SqlitePermanentConnection permanentConnection
                ? new SqlitePermanentlyConnectedDatabase( permanentConnection, builder, versionRecordsReader, newDbVersion )
                : new SqlitePersistentDatabase( connectionString, builder, versionRecordsReader, newDbVersion );

            return new SqlCreateDatabaseResult<SqliteDatabase>(
                database,
                exception,
                versions.Current,
                newDbVersion,
                appliedVersions,
                pendingVersions );
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
        SqlQueryDefinition<List<SqlDatabaseVersionRecord>> versionRecordsReader,
        ref SqlStatementExecutor statementExecutor)
    {
        using var command = connection.CreateCommand();
        command.CommandText = versionRecordsReader.Sql;
        var registeredVersionRecords = statementExecutor.ExecuteVersionHistoryQuery( command, versionRecordsReader.Executor );
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
    private static (SqliteTableBuilder VersionHistoryTable, VersionHistoryTypes Types)
        InitializeDatabaseBuilderWithVersionHistoryTable(
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

        var ordinal = columns.Create( VersionHistoryColumns.Ordinal ).SetType( intType );
        columns.Create( VersionHistoryColumns.VersionMajor ).SetType( intType );
        columns.Create( VersionHistoryColumns.VersionMinor ).SetType( intType );
        columns.Create( VersionHistoryColumns.VersionBuild ).SetType( intType ).MarkAsNullable();
        columns.Create( VersionHistoryColumns.VersionRevision ).SetType( intType ).MarkAsNullable();
        columns.Create( VersionHistoryColumns.Description ).SetType( stringType );
        columns.Create( VersionHistoryColumns.CommitDateUtc ).SetType( dateTimeType );
        columns.Create( VersionHistoryColumns.CommitDurationInTicks ).SetType( longType );
        table.SetPrimaryKey( ordinal.Asc() );

        return (table, new VersionHistoryTypes( intType, longType, stringType, dateTimeType ));
    }

    private static void CreateVersionHistoryTableInDatabaseIfNotExists(
        SqliteConnection connection,
        SqliteTableBuilder table,
        ref SqlStatementExecutor statementExecutor)
    {
        using ( var command = connection.CreateCommand() )
        {
            command.CommandText = $"SELECT EXISTS (SELECT * FROM sqlite_master WHERE type = 'table' AND name = '{table.FullName}');";
            var exists = statementExecutor.ExecuteVersionHistoryQuery( command, static cmd => Convert.ToBoolean( cmd.ExecuteScalar() ) );

            if ( ! exists )
            {
                using ( var transaction = CreateTransaction( command ) )
                {
                    var statements = table.Database.GetPendingStatements();
                    foreach ( var statement in statements )
                    {
                        command.CommandText = statement;
                        statementExecutor.ExecuteVersionHistoryNonQuery( command );
                    }

                    transaction.Commit();
                }

                command.Transaction = null;
            }
        }

        SetBuilderMode( table.Database, SqlDatabaseCreateMode.NoChanges );

        table.Remove();
        if ( table.Schema.CanRemove )
            table.Schema.Remove();

        ClearBuilderStatements( table.Database );
    }

    [Pure]
    [MethodImpl( MethodImplOptions.AggressiveInlining )]
    private static SqliteTransaction CreateTransaction(SqliteCommand command)
    {
        Assume.IsNotNull( command.Connection, nameof( command.Connection ) );
        var result = command.Connection.BeginTransaction( IsolationLevel.Serializable );
        command.Transaction = result;
        return result;
    }

    [Pure]
    private static SqlQueryDefinition<List<SqlDatabaseVersionRecord>> CreateVersionRecordsReader(SqliteTableBuilder table)
    {
        var query = $"SELECT * FROM \"{table.FullName}\" ORDER BY \"{VersionHistoryColumns.Ordinal}\" ASC;";
        return new SqlQueryDefinition<List<SqlDatabaseVersionRecord>>( query, Executor );

        [Pure]
        static List<SqlDatabaseVersionRecord> Executor(SqliteCommand c)
        {
            var result = new List<SqlDatabaseVersionRecord>();
            using var reader = c.ExecuteReader();

            if ( ! reader.Read() )
                return result;

            var iOrdinal = reader.GetOrdinal( VersionHistoryColumns.Ordinal );
            var iVersionMajor = reader.GetOrdinal( VersionHistoryColumns.VersionMajor );
            var iVersionMinor = reader.GetOrdinal( VersionHistoryColumns.VersionMinor );
            var iVersionBuild = reader.GetOrdinal( VersionHistoryColumns.VersionBuild );
            var iVersionRevision = reader.GetOrdinal( VersionHistoryColumns.VersionRevision );
            var iDescription = reader.GetOrdinal( VersionHistoryColumns.Description );
            var iCommitDateUtc = reader.GetOrdinal( VersionHistoryColumns.CommitDateUtc );
            var iCommitDurationInTicks = reader.GetOrdinal( VersionHistoryColumns.CommitDurationInTicks );

            do
            {
                var versionMajor = reader.GetInt32( iVersionMajor );
                var versionMinor = reader.GetInt32( iVersionMinor );
                var versionBuild = reader.IsDBNull( iVersionBuild ) ? (int?)null : reader.GetInt32( iVersionBuild );
                var versionRevision = versionBuild is null || reader.IsDBNull( iVersionRevision )
                    ? (int?)null
                    : reader.GetInt32( iVersionRevision );

                var record = new SqlDatabaseVersionRecord(
                    Ordinal: reader.GetInt32( iOrdinal ),
                    Version: versionBuild is null
                        ? new Version( versionMajor, versionMinor )
                        : versionRevision is null
                            ? new Version( versionMajor, versionMinor, versionBuild.Value )
                            : new Version( versionMajor, versionMinor, versionBuild.Value, versionRevision.Value ),
                    Description: reader.GetString( iDescription ),
                    CommitDateUtc: DateTime.Parse( reader.GetString( iCommitDateUtc ) ),
                    CommitDuration: TimeSpan.FromTicks( reader.GetInt64( iCommitDurationInTicks ) ) );

                result.Add( record );
            }
            while ( reader.Read() );

            return result;
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
        SqliteTableBuilder versionHistoryTable,
        SqlDatabaseVersionHistory.DatabaseComparisonResult versions,
        VersionHistoryTypes types,
        SqlDatabaseVersionHistoryPersistenceMode versionHistoryPersistenceMode,
        ref SqlStatementExecutor statementExecutor)
    {
        if ( versions.Uncommitted.Length == 0 )
            return (null, 0);

        var connection = ReinterpretCast.To<SqliteConnection>( connectionChangeEvent.Connection );

        using var statementCommand = connection.CreateCommand();
        var (preparePragmaText, restorePragmaText) = CreatePragmaCommandTexts( statementCommand, ref statementExecutor );

        using var insertVersionCommand = PrepareInsertVersionRecordCommand( connection, versionHistoryTable.FullName, types );
        var pOrdinal = insertVersionCommand.Parameters[0];
        var pVersionMajor = insertVersionCommand.Parameters[1];
        var pVersionMinor = insertVersionCommand.Parameters[2];
        var pVersionBuild = insertVersionCommand.Parameters[3];
        var pVersionRevision = insertVersionCommand.Parameters[4];
        var pDescription = insertVersionCommand.Parameters[5];
        var pCommitDateUtc = insertVersionCommand.Parameters[6];

        using var deleteVersionsCommand = versionHistoryPersistenceMode == SqlDatabaseVersionHistoryPersistenceMode.LastRecordOnly
            ? OptionalDisposable.Create( PrepareDeleteVersionRecordsCommand( connection, versionHistoryTable.FullName ) )
            : OptionalDisposable<SqliteCommand>.Empty;

        var builder = versionHistoryTable.Database;
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
                        statementCommand.CommandText = statement;
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

                    pOrdinal.Value = versionOrdinal;
                    pVersionMajor.Value = version.Value.Major;
                    pVersionMinor.Value = version.Value.Minor;
                    pVersionBuild.Value = version.Value.Build >= 0 ? version.Value.Build : DBNull.Value;
                    pVersionRevision.Value = version.Value.Revision >= 0 ? version.Value.Revision : DBNull.Value;
                    pDescription.Value = version.Description;
                    types.DateTimeType.SetParameter( pCommitDateUtc, DateTime.UtcNow );

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
                versionHistoryTable.FullName,
                CollectionsMarshal.AsSpan( elapsedTimes ),
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
    private static SqliteCommand PrepareInsertVersionRecordCommand(
        SqliteConnection connection,
        string versionTableName,
        VersionHistoryTypes types)
    {
        var query = new StringBuilder()
            .Append( "INSERT INTO" )
            .AppendTokenSeparator()
            .AppendName( versionTableName )
            .AppendTokenSeparator()
            .AppendElementsBegin();

        AppendColumnName( query, VersionHistoryColumns.Ordinal );
        AppendColumnName( query, VersionHistoryColumns.VersionMajor );
        AppendColumnName( query, VersionHistoryColumns.VersionMinor );
        AppendColumnName( query, VersionHistoryColumns.VersionBuild );
        AppendColumnName( query, VersionHistoryColumns.VersionRevision );
        AppendColumnName( query, VersionHistoryColumns.Description );
        AppendColumnName( query, VersionHistoryColumns.CommitDateUtc );

        query
            .AppendName( VersionHistoryColumns.CommitDurationInTicks )
            .AppendElementsEnd()
            .AppendLine()
            .Append( "VALUES" )
            .AppendTokenSeparator()
            .AppendElementsBegin();

        AppendParameter( query, "@p0" );
        AppendParameter( query, "@p1" );
        AppendParameter( query, "@p2" );
        AppendParameter( query, "@p3" );
        AppendParameter( query, "@p4" );
        AppendParameter( query, "@p5" );
        AppendParameter( query, "@p6" );

        query
            .Append( types.LongType.ToDbLiteral( 0 ) )
            .AppendElementsEnd()
            .AppendCommandEnd();

        var command = connection.CreateCommand();
        try
        {
            command.CommandText = query.ToString();
            command.Parameters.Add( new SqliteParameter( "@p0", SqliteType.Integer ) );
            command.Parameters.Add( new SqliteParameter( "@p1", SqliteType.Integer ) );
            command.Parameters.Add( new SqliteParameter( "@p2", SqliteType.Integer ) );
            command.Parameters.Add( new SqliteParameter( "@p3", SqliteType.Integer ) );
            command.Parameters.Add( new SqliteParameter( "@p4", SqliteType.Integer ) );
            command.Parameters.Add( new SqliteParameter( "@p5", SqliteType.Text ) );
            command.Parameters.Add( new SqliteParameter( "@p6", SqliteType.Text ) );
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }

        return command;

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static void AppendColumnName(StringBuilder builder, string name)
        {
            builder.AppendName( name ).AppendElementSeparator().AppendTokenSeparator();
        }

        [MethodImpl( MethodImplOptions.AggressiveInlining )]
        static void AppendParameter(StringBuilder builder, string name)
        {
            builder.Append( name ).AppendElementSeparator().AppendTokenSeparator();
        }
    }

    [Pure]
    private static SqliteCommand PrepareDeleteVersionRecordsCommand(SqliteConnection connection, string versionTableName)
    {
        var query = new StringBuilder()
            .Append( "DELETE FROM" )
            .AppendTokenSeparator()
            .AppendName( versionTableName )
            .AppendCommandEnd();

        var command = connection.CreateCommand();
        try
        {
            command.CommandText = query.ToString();
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
        string versionTableName,
        ReadOnlySpan<(int Ordinal, TimeSpan ElapsedTime)> elapsedTimes,
        ref SqlStatementExecutor statementExecutor)
    {
        if ( elapsedTimes.Length == 0 )
            return;

        using var command = PrepareUpdateVersionRecordCommitDurationCommand( connection, versionTableName );
        var pCommitDurationInTicks = command.Parameters[0];
        var pOrdinal = command.Parameters[1];

        using var transaction = CreateTransaction( command );

        foreach ( var (ordinal, elapsedTime) in elapsedTimes )
        {
            pCommitDurationInTicks.Value = elapsedTime.Ticks;
            pOrdinal.Value = ordinal;
            statementExecutor.ExecuteVersionHistoryNonQuery( command );
        }

        transaction.Commit();
    }

    [Pure]
    private static SqliteCommand PrepareUpdateVersionRecordCommitDurationCommand(SqliteConnection connection, string versionTableName)
    {
        var query = new StringBuilder()
            .Append( "UPDATE" )
            .AppendTokenSeparator()
            .AppendName( versionTableName )
            .AppendLine()
            .Append( "SET" )
            .AppendTokenSeparator()
            .AppendName( VersionHistoryColumns.CommitDurationInTicks )
            .AppendTokenSeparator()
            .Append( '=' )
            .AppendTokenSeparator()
            .AppendLine( "@p0" )
            .Append( "WHERE" )
            .AppendTokenSeparator()
            .AppendName( VersionHistoryColumns.Ordinal )
            .AppendTokenSeparator()
            .Append( '=' )
            .AppendTokenSeparator()
            .Append( "@p1" )
            .AppendCommandEnd();

        var command = connection.CreateCommand();
        try
        {
            command.CommandText = query.ToString();
            command.Parameters.Add( new SqliteParameter( "@p0", SqliteType.Integer ) );
            command.Parameters.Add( new SqliteParameter( "@p1", SqliteType.Integer ) );
            command.Prepare();
        }
        catch
        {
            command.Dispose();
            throw;
        }

        return command;
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

    private static class VersionHistoryColumns
    {
        public const string Ordinal = nameof( Ordinal );
        public const string VersionMajor = nameof( VersionMajor );
        public const string VersionMinor = nameof( VersionMinor );
        public const string VersionBuild = nameof( VersionBuild );
        public const string VersionRevision = nameof( VersionRevision );
        public const string Description = nameof( Description );
        public const string CommitDateUtc = nameof( CommitDateUtc );
        public const string CommitDurationInTicks = nameof( CommitDurationInTicks );
    }

    private readonly record struct VersionHistoryTypes(
        SqliteColumnTypeDefinition<int> IntType,
        SqliteColumnTypeDefinition<long> LongType,
        SqliteColumnTypeDefinition<string> StringType,
        SqliteColumnTypeDefinition<DateTime> DateTimeType);

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
