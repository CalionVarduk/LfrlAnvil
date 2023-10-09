using System.Collections.Generic;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;
using SqliteConnection = Microsoft.Data.Sqlite.SqliteConnection;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

public sealed class SqliteDatabaseMock : SqliteDatabase
{
    public SqliteDatabaseMock(SqliteDatabaseBuilder builder)
        : base(
            builder,
            new SqlQueryDefinition<List<SqlDatabaseVersionRecord>>( string.Empty, _ => new List<SqlDatabaseVersionRecord>() ),
            new Version() ) { }

    public override SqliteConnection Connect()
    {
        throw new NotSupportedException();
    }
}
