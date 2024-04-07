using System;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Extensions;

public static class SqlColumnTypeDefinitionExtensions
{
    [Pure]
    public static ISqlColumnTypeDefinition<T> GetByType<T>(this ISqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( ISqlColumnTypeDefinition<T> )provider.GetByType( typeof( T ) );
    }

    [Pure]
    public static ISqlColumnTypeDefinition<T>? TryGetByType<T>(this ISqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( ISqlColumnTypeDefinition<T>? )provider.TryGetByType( typeof( T ) );
    }

    [Pure]
    public static SqlColumnTypeDefinition<T> GetByType<T>(this SqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( SqlColumnTypeDefinition<T> )provider.GetByType( typeof( T ) );
    }

    [Pure]
    public static SqlColumnTypeDefinition<T>? TryGetByType<T>(this SqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( SqlColumnTypeDefinition<T>? )provider.TryGetByType( typeof( T ) );
    }

    [Pure]
    public static object? TryToNullableParameterValue(this ISqlColumnTypeDefinition definition, object? value)
    {
        return value is null ? DBNull.Value : definition.TryToParameterValue( value );
    }

    [Pure]
    public static object ToNullableParameterValue<T>(this ISqlColumnTypeDefinition<T> definition, T? value)
        where T : class
    {
        return value is null ? DBNull.Value : definition.ToParameterValue( value );
    }

    [Pure]
    public static object ToNullableParameterValue<T>(this ISqlColumnTypeDefinition<T> definition, T? value)
        where T : struct
    {
        return value is null ? DBNull.Value : definition.ToParameterValue( value.Value );
    }
}
