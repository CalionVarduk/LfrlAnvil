using System.Threading;
using System.Threading.Tasks;
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
            builder,
            new SqlQueryReader<SqlDatabaseVersionRecord>( new SqlDialect( "foo" ), (_, _) => default ).Bind( string.Empty ),
            new Version() ) { }

    public override SqliteConnection Connect()
    {
        throw new NotSupportedException();
    }

    public override ValueTask<SqliteConnection> ConnectAsync(CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}
