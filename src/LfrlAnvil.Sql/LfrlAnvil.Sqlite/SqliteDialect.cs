using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite;

/// <summary>
/// Contains an <see cref="SqlDialect"/> instance associated with SQLite.
/// </summary>
public static class SqliteDialect
{
    /// <summary>
    /// <see cref="SqlDialect"/> instance associated with SQLite.
    /// </summary>
    public static readonly SqlDialect Instance = new SqlDialect( "SQLite" );
}
