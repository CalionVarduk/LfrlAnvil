using System;
using System.Diagnostics.Contracts;
using System.Threading;
using System.Threading.Tasks;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePermanentlyConnectedDatabase : SqliteDatabase
{
    private readonly SqlitePermanentConnection _connection;

    internal SqlitePermanentlyConnectedDatabase(
        SqlitePermanentConnection connection,
        SqliteDatabaseBuilder builder,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery,
        Version version)
        : base( builder, versionRecordsQuery, version )
    {
        _connection = connection;
    }

    public override void Dispose()
    {
        base.Dispose();
        _connection.Close();
    }

    [Pure]
    public override Microsoft.Data.Sqlite.SqliteConnection Connect()
    {
        return _connection;
    }

    [Pure]
    public override ValueTask<Microsoft.Data.Sqlite.SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult<Microsoft.Data.Sqlite.SqliteConnection>( _connection );
    }
}
