using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql;

/// <summary>
/// Contains an <see cref="SqlDialect"/> instance associated with PostgreSQL.
/// </summary>
public static class PostgreSqlDialect
{
    /// <summary>
    /// <see cref="SqlDialect"/> instance associated with PostgreSQL.
    /// </summary>
    public static readonly SqlDialect Instance = new SqlDialect( "PostgreSql" );
}
