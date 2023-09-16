using LfrlAnvil.Sql;

namespace LfrlAnvil.Sqlite.Extensions;

public static class SqlDatabaseFactoryProviderExtensions
{
    public static SqlDatabaseFactoryProvider RegisterSqlite(this SqlDatabaseFactoryProvider provider, bool isConnectionPermanent = false)
    {
        return provider.RegisterFactory( new SqliteDatabaseFactory( isConnectionPermanent ) );
    }
}
