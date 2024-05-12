using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql.Extensions;

/// <summary>
/// Contains <see cref="ISqlColumnTypeDefinition"/> extension methods.
/// </summary>
public static class SqlColumnTypeDefinitionExtensions
{
    /// <summary>
    /// Returns a column type definition associated with the provided type.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <typeparam name="T">Runtime type to get type definition for.</typeparam>
    /// <returns>Column type definition associated with the provided type.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When column type definition for the provided type does not exist.
    /// </exception>
    [Pure]
    public static ISqlColumnTypeDefinition<T> GetByType<T>(this ISqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( ISqlColumnTypeDefinition<T> )provider.GetByType( typeof( T ) );
    }

    /// <summary>
    /// Attempts to return a column type definition associated with the provided type.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <typeparam name="T">Runtime type to get type definition for.</typeparam>
    /// <returns>Column type definition associated with the provided type
    /// or null when column type definition for the provided type does not exist.
    /// </returns>
    [Pure]
    public static ISqlColumnTypeDefinition<T>? TryGetByType<T>(this ISqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( ISqlColumnTypeDefinition<T>? )provider.TryGetByType( typeof( T ) );
    }

    /// <summary>
    /// Returns a column type definition associated with the provided type.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <typeparam name="T">Runtime type to get type definition for.</typeparam>
    /// <returns>Column type definition associated with the provided type.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When column type definition for the provided type does not exist.
    /// </exception>
    [Pure]
    public static SqlColumnTypeDefinition<T> GetByType<T>(this SqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( SqlColumnTypeDefinition<T> )provider.GetByType( typeof( T ) );
    }

    /// <summary>
    /// Attempts to return a column type definition associated with the provided type.
    /// </summary>
    /// <param name="provider">Source provider.</param>
    /// <typeparam name="T">Runtime type to get type definition for.</typeparam>
    /// <returns>Column type definition associated with the provided type
    /// or null when column type definition for the provided type does not exist.
    /// </returns>
    [Pure]
    public static SqlColumnTypeDefinition<T>? TryGetByType<T>(this SqlColumnTypeDefinitionProvider provider)
        where T : notnull
    {
        return ( SqlColumnTypeDefinition<T>? )provider.TryGetByType( typeof( T ) );
    }

    /// <summary>
    /// Attempts to create an object from the provided nullable <paramref name="value"/>
    /// that can be used to set DB parameter's <see cref="IDataParameter.Value"/> with.
    /// </summary>
    /// <param name="definition">Source definition.</param>
    /// <param name="value">Nullable value to convert.</param>
    /// <returns>
    /// Converted <paramref name="value"/>
    /// or <see cref="DBNull"/> instance when <paramref name="value"/> is null
    /// or null when it is not of the specified <see cref="ISqlColumnTypeDefinition.RuntimeType"/>.
    /// </returns>
    [Pure]
    public static object? TryToNullableParameterValue(this ISqlColumnTypeDefinition definition, object? value)
    {
        return value is null ? DBNull.Value : definition.TryToParameterValue( value );
    }

    /// <summary>
    /// Creates an object from the provided nullable <paramref name="value"/>
    /// that can be used to set DB parameter's <see cref="IDataParameter.Value"/> with.
    /// </summary>
    /// <param name="definition">Source definition.</param>
    /// <param name="value">Value to convert.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Converted <paramref name="value"/> or <see cref="DBNull"/> instance when <paramref name="value"/> is null.</returns>
    [Pure]
    public static object ToNullableParameterValue<T>(this ISqlColumnTypeDefinition<T> definition, T? value)
        where T : class
    {
        return value is null ? DBNull.Value : definition.ToParameterValue( value );
    }

    /// <summary>
    /// Creates an object from the provided nullable <paramref name="value"/>
    /// that can be used to set DB parameter's <see cref="IDataParameter.Value"/> with.
    /// </summary>
    /// <param name="definition">Source definition.</param>
    /// <param name="value">Value to convert.</param>
    /// <typeparam name="T">Value type.</typeparam>
    /// <returns>Converted <paramref name="value"/> or <see cref="DBNull"/> instance when <paramref name="value"/> is null.</returns>
    [Pure]
    public static object ToNullableParameterValue<T>(this ISqlColumnTypeDefinition<T> definition, T? value)
        where T : struct
    {
        return value is null ? DBNull.Value : definition.ToParameterValue( value.Value );
    }
}
