using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePermanentlyConnectedDatabase : SqliteDatabase
{
    private readonly SqlitePermanentConnection _connection;

    internal SqlitePermanentlyConnectedDatabase(
        SqlitePermanentConnection connection,
        SqliteConnectionStringBuilder connectionStringBuilder,
        SqliteDatabaseBuilder builder,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>> connectionChangeCallbacks)
        : base( connectionStringBuilder, builder, version, versionRecordsQuery )
    {
        _connection = connection;
        InitializeConnectionEventHandlers( connection, connectionChangeCallbacks );
    }

    public override void Dispose()
    {
        base.Dispose();
        _connection.Close();
    }

    [Pure]
    protected override SqliteConnection CreateConnection()
    {
        return _connection;
    }
}
