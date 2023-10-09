using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Internal;

internal sealed class SqlitePermanentlyConnectedDatabase : SqliteDatabase
{
    private readonly SqlitePermanentConnection _connection;

    internal SqlitePermanentlyConnectedDatabase(
        SqlitePermanentConnection connection,
        SqliteDatabaseBuilder builder,
        SqlQueryDefinition<List<SqlDatabaseVersionRecord>> versionRecordsReader,
        Version version)
        : base( builder, versionRecordsReader, version )
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
}
