using LfrlAnvil.Sql;

namespace LfrlAnvil.MySql;

public static class MySqlDialect
{
    public static readonly SqlDialect Instance = new SqlDialect( "MySql" );
}
