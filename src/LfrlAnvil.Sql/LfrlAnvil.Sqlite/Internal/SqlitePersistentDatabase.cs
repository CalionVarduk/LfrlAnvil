using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePersistentDatabase : SqliteDatabase
{
    private readonly ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> _connectionChangeCallbacks;
    private readonly string _connectionString;

    internal SqlitePersistentDatabase(
        string connectionString,
        SqliteConnectionStringBuilder connectionStringBuilder,
        SqliteDatabaseBuilder builder,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> connectionChangeCallbacks)
        : base( connectionStringBuilder, builder, version, versionRecordsQuery )
    {
        Assume.Equals( connectionString, connectionStringBuilder.ToString() );
        _connectionString = connectionString;
        _connectionChangeCallbacks = connectionChangeCallbacks;
    }

    [Pure]
    protected override SqliteConnection CreateConnection()
    {
        var result = new SqliteConnection( _connectionString );
        InitializeConnectionEventHandlers( result, _connectionChangeCallbacks );
        return result;
    }
}
