using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Extensions;

public static class SqlDatabaseFactoryProviderExtensions
{
    public static SqlDatabaseFactoryProvider RegisterSqlite(this SqlDatabaseFactoryProvider provider)
    {
        return provider.RegisterFactory( new SqliteDatabaseFactory() );
    }
}
