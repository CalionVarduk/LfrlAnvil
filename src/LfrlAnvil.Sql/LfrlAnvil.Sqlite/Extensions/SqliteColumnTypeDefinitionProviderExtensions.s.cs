using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sqlite.Extensions;

public static class SqliteColumnTypeDefinitionProviderExtensions
{
    [Pure]
    public static SqliteColumnTypeDefinition<T> GetByType<T>(this SqliteColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return (SqliteColumnTypeDefinition<T>)provider.GetByType( typeof( T ) );
    }
}
