using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Extensions;

/// <summary>
/// Contains <see cref="SqlDatabaseFactoryProvider"/> extension methods.
/// </summary>
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public static class SqlDatabaseFactoryProviderExtensions
{
    /// <summary>
    /// Registers a new <see cref="SqliteDatabaseFactory"/> instance in the <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <param name="options">
    /// Optional <see cref="SqliteDatabaseFactoryOptions"/>. Equal to <see cref="SqliteDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    /// <returns><paramref name="provider"/>.</returns>
    public static SqlDatabaseFactoryProvider RegisterSqlite(
        this SqlDatabaseFactoryProvider provider,
        SqliteDatabaseFactoryOptions? options = null)
    {
        return provider.RegisterFactory( new SqliteDatabaseFactory( options ) );
    }
}
