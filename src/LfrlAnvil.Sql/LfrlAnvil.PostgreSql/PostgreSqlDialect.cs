using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql;

public static class PostgreSqlDialect
{
    public static readonly SqlDialect Instance = new SqlDialect( "PostgreSql" );
}
