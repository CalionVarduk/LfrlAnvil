using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using LfrlAnvil.Diagnostics;
using LfrlAnvil.Sql;
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
        List<Action<SqlDatabaseConnectionChangeEvent>>? connectionChangeCallbacks = null;
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

            var versionRecordsReader = CreateVersionRecordsReader( versionHistoryTable );

            if ( ! VersionHistoryTableExistsInDatabase( connection, versionHistoryTable.FullName ) )
                CreateVersionHistoryTableInDatabase( connection, versionHistoryTable );

            RemoveVersionHistoryTable( versionHistoryTable );

            var versions = CompareVersionHistoryToDatabase( connection, versionHistory, versionRecordsReader );

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
                    options.VersionHistoryPersistenceMode ),
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
        Func<SqliteCommand, List<SqlDatabaseVersionRecord>> versionRecordsReader)
    {
        using var command = connection.CreateCommand();
        var registeredVersionRecords = versionRecordsReader( command );
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
    private static bool VersionHistoryTableExistsInDatabase(SqliteConnection connection, string tableName)
    {
        using var command = connection.CreateCommand();
        command.CommandText = $"SELECT EXISTS (SELECT * FROM sqlite_master WHERE type = 'table' AND name = '{tableName}');";
        var exists = Convert.ToBoolean( command.ExecuteScalar() );
        return exists;
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

    private static void CreateVersionHistoryTableInDatabase(SqliteConnection connection, SqliteTableBuilder table)
    {
        using var command = connection.CreateCommand();
        using var transaction = CreateTransaction( command );

        foreach ( var statement in table.Database.GetPendingStatements() )
        {
            command.CommandText = statement;
            command.ExecuteNonQuery();
        }

        transaction.Commit();
    }

    private static void RemoveVersionHistoryTable(SqliteTableBuilder table)
    {
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
    private static Func<SqliteCommand, List<SqlDatabaseVersionRecord>> CreateVersionRecordsReader(SqliteTableBuilder table)
    {
        var query = $"SELECT * FROM \"{table.FullName}\" ORDER BY \"{VersionHistoryColumns.Ordinal}\" ASC;";

        return c =>
        {
            var result = new List<SqlDatabaseVersionRecord>();

            c.CommandText = query;
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
        };
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
        SqlDatabaseVersionHistoryPersistenceMode versionHistoryPersistenceMode)
    {
        if ( versions.Uncommitted.Length == 0 )
            return (null, 0);

        var connection = ReinterpretCast.To<SqliteConnection>( connectionChangeEvent.Connection );
        using var statementCommand = connection.CreateCommand();
        var (preparePragmaText, restorePragmaText) = CreatePragmaCommandTexts( statementCommand );

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

            try
            {
                if ( preparePragmaText.Length > 0 )
                {
                    statementCommand.CommandText = preparePragmaText;
                    statementCommand.ExecuteNonQuery();
                    pragmaSwapped = true;
                }

                try
                {
                    using var transaction = CreateTransaction( statementCommand );
                    insertVersionCommand.Transaction = transaction;

                    foreach ( var statement in statements )
                    {
                        statementCommand.CommandText = statement;
                        statementCommand.ExecuteNonQuery();
                    }

                    foreach ( var tableName in builder.ChangeTracker.ModifiedTableNames )
                    {
                        statementCommand.CommandText = $"PRAGMA foreign_key_check('{tableName}');";
                        using var reader = statementCommand.ExecuteReader();
                        if ( reader.Read() )
                            fkCheckFailures.Add( tableName );
                    }

                    if ( fkCheckFailures.Count > 0 )
                        throw new SqliteForeignKeyCheckException( version.Value, fkCheckFailures );

                    if ( deleteVersionsCommand.Value is not null )
                    {
                        deleteVersionsCommand.Value.Transaction = transaction;
                        deleteVersionsCommand.Value.ExecuteNonQuery();
                    }

                    pOrdinal.Value = versionOrdinal;
                    pVersionMajor.Value = version.Value.Major;
                    pVersionMinor.Value = version.Value.Minor;
                    pVersionBuild.Value = version.Value.Build >= 0 ? version.Value.Build : DBNull.Value;
                    pVersionRevision.Value = version.Value.Revision >= 0 ? version.Value.Revision : DBNull.Value;
                    pDescription.Value = version.Description;
                    types.DateTimeType.SetParameter( pCommitDateUtc, DateTime.UtcNow );

                    insertVersionCommand.ExecuteNonQuery();
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
                        statementCommand.CommandText = restorePragmaText;
                        statementCommand.ExecuteNonQuery();
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
            UpdateVersionRecordRangeElapsedTime( connection, versionHistoryTable.FullName, CollectionsMarshal.AsSpan( elapsedTimes ) );
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
    private static (string Prepare, string Restore) CreatePragmaCommandTexts(SqliteCommand command)
    {
        bool areForeignKeysEnabled;
        bool isLegacyAlterTableEnabled;
        command.CommandText = "PRAGMA foreign_keys; PRAGMA legacy_alter_table;";

        using ( var reader = command.ExecuteReader() )
        {
            reader.Read();
            areForeignKeysEnabled = reader.GetBoolean( 0 );
            reader.NextResult();
            reader.Read();
            isLegacyAlterTableEnabled = reader.GetBoolean( 0 );
        }

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
        ReadOnlySpan<(int Ordinal, TimeSpan ElapsedTime)> elapsedTimes)
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
            command.ExecuteNonQuery();
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

    SqlCreateDatabaseResult<ISqlDatabase> ISqlDatabaseFactory.Create(
        string connectionString,
        SqlDatabaseVersionHistory versionHistory,
        SqlCreateDatabaseOptions options)
    {
        return Create( connectionString, versionHistory, options );
    }
}
