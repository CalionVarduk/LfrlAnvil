using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlColumnTypeDefinitionExtensions
{
    [Pure]
    public static ISqlColumnTypeDefinition<T> GetByType<T>(this ISqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return (ISqlColumnTypeDefinition<T>)provider.GetByType( typeof( T ) );
    }

    public static bool TrySetNullableParameter(this ISqlColumnTypeDefinition definition, IDbDataParameter parameter, object? value)
    {
        if ( value is not null )
            return definition.TrySetParameter( parameter, value );

        definition.SetNullParameter( parameter );
        return true;
    }

    public static void SetNullableParameter<T>(this ISqlColumnTypeDefinition<T> definition, IDbDataParameter parameter, T? value)
        where T : class
    {
        if ( value is null )
            definition.SetNullParameter( parameter );
        else
            definition.SetParameter( parameter, value );
    }

    public static void SetNullableParameter<T>(this ISqlColumnTypeDefinition<T> definition, IDbDataParameter parameter, T? value)
        where T : struct
    {
        if ( value is null )
            definition.SetNullParameter( parameter );
        else
            definition.SetParameter( parameter, value.Value );
    }
}
