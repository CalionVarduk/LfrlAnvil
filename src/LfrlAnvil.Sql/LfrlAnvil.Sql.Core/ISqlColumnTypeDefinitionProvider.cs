using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a collection of column type definitions.
/// </summary>
public interface ISqlColumnTypeDefinitionProvider
{
    /// <summary>
    /// Specifies the SQL dialect of this provider.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Returns a collection of all column type definitions identifiable by <see cref="Type"/> instances.
    /// </summary>
    /// <returns>Collection of all column type definitions identifiable by <see cref="Type"/> instances.</returns>
    [Pure]
    IReadOnlyCollection<ISqlColumnTypeDefinition> GetTypeDefinitions();

    /// <summary>
    /// Returns a collection of all default column type definitions identifiable by <see cref="ISqlDataType"/> instances.
    /// </summary>
    /// <returns>Collection of all default column type definitions identifiable by <see cref="ISqlDataType"/> instances.</returns>
    [Pure]
    IReadOnlyCollection<ISqlColumnTypeDefinition> GetDataTypeDefinitions();

    /// <summary>
    /// Returns a default column type definition associated with the provided <paramref name="dataType"/>.
    /// </summary>
    /// <param name="dataType">Data type to get default type definition for.</param>
    /// <returns>Default column type definition associated with the provided <paramref name="dataType"/>.</returns>
    [Pure]
    ISqlColumnTypeDefinition GetByDataType(ISqlDataType dataType);

    /// <summary>
    /// Returns a column type definition associated with the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Runtime type to get type definition for.</param>
    /// <returns>Column type definition associated with the provided <paramref name="type"/>.</returns>
    /// <exception cref="KeyNotFoundException">
    /// When column type definition for the provided <paramref name="type"/> does not exist.
    /// </exception>
    [Pure]
    ISqlColumnTypeDefinition GetByType(Type type);

    /// <summary>
    /// Attempts to return a column type definition associated with the provided <paramref name="type"/>.
    /// </summary>
    /// <param name="type">Runtime type to get type definition for.</param>
    /// <returns>Column type definition associated with the provided <paramref name="type"/>
    /// or null when column type definition for the provided <paramref name="type"/> does not exist.
    /// </returns>
    [Pure]
    ISqlColumnTypeDefinition? TryGetByType(Type type);

    /// <summary>
    /// Checks whether or not the specified <paramref name="definition"/> belongs to this provider.
    /// </summary>
    /// <param name="definition">Definition to check.</param>
    /// <returns><b>true</b> when <paramref name="definition"/> belongs to this provider, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(ISqlColumnTypeDefinition definition);
}
