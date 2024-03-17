using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

public static class SqliteDatabaseMock
{
    [Pure]
    public static SqliteDatabase Create(SqliteDatabaseBuilder builder)
    {
        var connectionString = new SqliteConnectionStringBuilder { DataSource = SqliteHelpers.MemoryDataSource };
        var connector = new SqliteDatabasePermanentConnector(
            new SqlitePermanentConnection( connectionString.ToString() ),
            connectionString,
            new DbConnectionEventHandler( ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>>.Empty ) );

        var result = new SqliteDatabase(
            builder,
            connector,
            new Version(),
            new SqlQueryReader<SqlDatabaseVersionRecord>( SqliteDialect.Instance, (_, _) => default ).Bind( string.Empty ) );

        connector.SetDatabase( result );
        return result;
    }
}
