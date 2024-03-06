using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Objects.Builders;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Tests.Helpers;

public sealed class SqliteDatabaseMock : SqliteDatabase
{
    public SqliteDatabaseMock(SqliteDatabaseBuilder builder)
        : base(
            new SqliteConnectionStringBuilder( "DataSource=:memory:" ),
            builder,
            new Version(),
            new SqlQueryReader<SqlDatabaseVersionRecord>( new SqlDialect( "foo" ), (_, _) => default ).Bind( string.Empty ) ) { }

    protected override SqliteConnection CreateConnection()
    {
        throw new NotSupportedException();
    }
}
