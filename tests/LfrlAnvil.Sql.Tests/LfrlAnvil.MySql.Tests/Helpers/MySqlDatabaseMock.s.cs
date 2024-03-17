using System.Diagnostics.Contracts;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
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
            builder,
            new MySqlDatabaseConnector(
                new MySqlConnectionStringBuilder(),
                new DbConnectionEventHandler( ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>>.Empty ) ),
            new Version(),
            new SqlQueryReader<SqlDatabaseVersionRecord>( MySqlDialect.Instance, (_, _) => default ).Bind( string.Empty ) );
    }
}
