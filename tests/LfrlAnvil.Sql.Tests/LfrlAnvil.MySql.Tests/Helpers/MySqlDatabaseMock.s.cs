using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using MySqlConnector;

namespace LfrlAnvil.MySql.Tests.Helpers;

public static class MySqlDatabaseMock
{
    [Pure]
    public static MySqlDatabase Create(MySqlDatabaseBuilder builder)
    {
        return new MySqlDatabase(
            new MySqlConnectionStringBuilder(),
            builder,
            new Version(),
            new SqlQueryReader<SqlDatabaseVersionRecord>( new SqlDialect( "foo" ), (_, _) => default ).Bind( string.Empty ),
            ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>>.Empty );
    }
}
