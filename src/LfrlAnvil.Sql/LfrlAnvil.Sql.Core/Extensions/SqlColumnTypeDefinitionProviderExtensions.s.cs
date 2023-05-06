using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlColumnTypeDefinitionProviderExtensions
{
    [Pure]
    public static ISqlColumnTypeDefinition<T> GetByType<T>(this ISqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return (ISqlColumnTypeDefinition<T>)provider.GetByType( typeof( T ) );
    }
}
