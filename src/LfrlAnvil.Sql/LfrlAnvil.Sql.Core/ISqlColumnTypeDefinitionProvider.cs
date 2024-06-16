// Copyright 2024 Łukasz Furlepa
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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
