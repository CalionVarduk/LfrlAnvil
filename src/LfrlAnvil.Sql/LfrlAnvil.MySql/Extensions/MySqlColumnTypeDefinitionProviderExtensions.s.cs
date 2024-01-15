using System.Diagnostics.Contracts;

namespace LfrlAnvil.MySql.Extensions;

public static class MySqlColumnTypeDefinitionProviderExtensions
{
    [Pure]
    public static MySqlColumnTypeDefinition<T> GetByType<T>(this MySqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return (MySqlColumnTypeDefinition<T>)provider.GetByType( typeof( T ) );
    }
}
