using LfrlAnvil.Sql;

namespace LfrlAnvil.MySql;

/// <summary>
/// Contains an <see cref="SqlDialect"/> instance associated with MySQL.
/// </summary>
public static class MySqlDialect
{
    /// <summary>
    /// <see cref="SqlDialect"/> instance associated with MySQL.
    /// </summary>
    public static readonly SqlDialect Instance = new SqlDialect( "MySql" );
}
