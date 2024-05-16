using LfrlAnvil.Sql;

namespace LfrlAnvil.MySql.Extensions;

/// <summary>
/// Contains <see cref="SqlDatabaseFactoryProvider"/> extension methods.
/// </summary>
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public static class SqlDatabaseFactoryProviderExtensions
{
    /// <summary>
    /// Registers a new <see cref="MySqlDatabaseFactory"/> instance in the <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <param name="options">
    /// Optional <see cref="MySqlDatabaseFactoryOptions"/>. Equal to <see cref="MySqlDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    /// <returns><paramref name="provider"/>.</returns>
    public static SqlDatabaseFactoryProvider RegisterMySql(
        this SqlDatabaseFactoryProvider provider,
        MySqlDatabaseFactoryOptions? options = null)
    {
        return provider.RegisterFactory( new MySqlDatabaseFactory( options ) );
    }
}
