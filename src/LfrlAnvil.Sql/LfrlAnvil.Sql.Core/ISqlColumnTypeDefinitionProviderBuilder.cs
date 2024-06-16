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
using System.Diagnostics.Contracts;

namespace LfrlAnvil.Sql;

/// <summary>
/// Represents a builder of <see cref="ISqlColumnTypeDefinitionProvider"/> instances.
/// </summary>
public interface ISqlColumnTypeDefinitionProviderBuilder
{
    /// <summary>
    /// Specifies the SQL dialect of this builder.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Checks whether or not a column type definition for the provided <paramref name="type"/> has been registered.
    /// </summary>
    /// <param name="type">Type to check.</param>
    /// <returns><b>true</b> when column type definition exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(Type type);

    /// <summary>
    /// Adds or updates the provided column type <paramref name="definition"/>
    /// by its <see cref="ISqlColumnTypeDefinition.RuntimeType"/> to this builder.
    /// </summary>
    /// <param name="definition">Definition to register.</param>
    /// <returns><b>this</b>.</returns>
    /// <exception cref="ArgumentException">When definition's dialect is not the same as this builder's dialect.</exception>
    ISqlColumnTypeDefinitionProviderBuilder Register(ISqlColumnTypeDefinition definition);

    /// <summary>
    /// Creates a new <see cref="ISqlColumnTypeDefinitionProvider"/> instance.
    /// </summary>
    /// <returns>New <see cref="ISqlColumnTypeDefinitionProvider"/> instance.</returns>
    [Pure]
    ISqlColumnTypeDefinitionProvider Build();
}
