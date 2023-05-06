using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite;

public static class SqliteDialect
{
    public static readonly SqlDialect Instance = new SqlDialect( "SQLite" );
}
