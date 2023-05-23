using System.Collections.Generic;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

public sealed class SqliteDatabaseMock : SqliteDatabase
{
    public SqliteDatabaseMock(SqliteDatabaseBuilder builder)
        : base(
            builder,
            _ => new List<SqlDatabaseVersionRecord>(),
            new Version() ) { }

    public override SqliteConnection Connect()
    {
        throw new NotSupportedException();
    }
}
