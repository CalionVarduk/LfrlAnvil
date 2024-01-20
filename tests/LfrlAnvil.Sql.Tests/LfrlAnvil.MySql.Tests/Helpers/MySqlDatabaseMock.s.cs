using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.MySql.Tests.Helpers;

public static class MySqlDatabaseMock
{
    [Pure]
    public static MySqlDatabase Create(MySqlDatabaseBuilder builder)
    {
        return new MySqlDatabase(
            string.Empty,
            builder,
            new SqlQueryReader<SqlDatabaseVersionRecord>( new SqlDialect( "foo" ), (_, _) => default ).Bind( string.Empty ),
            new Version() );
    }
}
