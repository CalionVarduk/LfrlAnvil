using System.Diagnostics.Contracts;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Internal;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using Npgsql;

namespace LfrlAnvil.PostgreSql.Tests.Helpers;

public static class PostgreSqlDatabaseMock
{
    [Pure]
    public static PostgreSqlDatabase Create(PostgreSqlDatabaseBuilder builder)
    {
        return new PostgreSqlDatabase(
            builder,
            new PostgreSqlDatabaseConnector(
                new NpgsqlConnectionStringBuilder(),
                new DbConnectionEventHandler( ReadOnlyArray<Action<SqlDatabaseConnectionChangeEvent>>.Empty ) ),
            new Version(),
            new SqlQueryReader<SqlDatabaseVersionRecord>( PostgreSqlDialect.Instance, (_, _) => default ).Bind( string.Empty ) );
    }
}
