using LfrlAnvil.Sql;

namespace LfrlAnvil.PostgreSql.Extensions;

public static class SqlDatabaseFactoryProviderExtensions
{
    public static SqlDatabaseFactoryProvider RegisterPostgreSql(
        this SqlDatabaseFactoryProvider provider,
        PostgreSqlDatabaseFactoryOptions? options = null)
    {
        return provider.RegisterFactory( new PostgreSqlDatabaseFactory( options ) );
    }
}
