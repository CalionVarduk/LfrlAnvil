using LfrlAnvil.Sql;

namespace LfrlAnvil.MySql.Extensions;

public static class SqlDatabaseFactoryProviderExtensions
{
    public static SqlDatabaseFactoryProvider RegisterMySql(this SqlDatabaseFactoryProvider provider)
    {
        return provider.RegisterFactory( new MySqlDatabaseFactory() );
    }
}
