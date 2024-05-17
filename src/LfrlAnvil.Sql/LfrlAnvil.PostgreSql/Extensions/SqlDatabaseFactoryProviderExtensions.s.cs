using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql.Extensions;

/// <summary>
/// Contains <see cref="SqlDatabaseFactoryProvider"/> extension methods.
/// </summary>
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public static class SqlDatabaseFactoryProviderExtensions
{
    /// <summary>
    /// Registers a new <see cref="PostgreSqlDatabaseFactory"/> instance in the <paramref name="provider"/>.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <param name="options">
    /// Optional <see cref="PostgreSqlDatabaseFactoryOptions"/>. Equal to <see cref="PostgreSqlDatabaseFactoryOptions.Default"/> by default.
    /// </param>
    /// <returns><paramref name="provider"/>.</returns>
    public static SqlDatabaseFactoryProvider RegisterPostgreSql(
        this SqlDatabaseFactoryProvider provider,
        PostgreSqlDatabaseFactoryOptions? options = null)
    {
        return provider.RegisterFactory( new PostgreSqlDatabaseFactory( options ) );
    }
}
