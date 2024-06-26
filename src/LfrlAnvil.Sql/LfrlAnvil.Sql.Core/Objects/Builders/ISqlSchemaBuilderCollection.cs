﻿// Copyright 2024 Łukasz Furlepa
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

using System.Collections.Generic;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Exceptions;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents a collection of SQL schema builders.
/// </summary>
public interface ISqlSchemaBuilderCollection : IReadOnlyCollection<ISqlSchemaBuilder>
{
    /// <summary>
    /// Database that this collection belongs to.
    /// </summary>
    ISqlDatabaseBuilder Database { get; }

    /// <summary>
    /// Default schema.
    /// </summary>
    ISqlSchemaBuilder Default { get; }

    /// <summary>
    /// Checks whether or not a schema with the provided <paramref name="name"/> exists.
    /// </summary>
    /// <param name="name">Name to check.</param>
    /// <returns><b>true</b> when schema exists, otherwise <b>false</b>.</returns>
    [Pure]
    bool Contains(string name);

    /// <summary>
    /// Returns a schema with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the schema to return.</param>
    /// <returns>Existing schema.</returns>
    /// <exception cref="KeyNotFoundException">When schema does not exist.</exception>
    [Pure]
    ISqlSchemaBuilder Get(string name);

    /// <summary>
    /// Attempts to return a schema with the provided <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the schema to return.</param>
    /// <returns>Existing schema or null when schema does not exist.</returns>
    [Pure]
    ISqlSchemaBuilder? TryGet(string name);

    /// <summary>
    /// Creates a new schema builder.
    /// </summary>
    /// <param name="name">Name of the schema.</param>
    /// <returns>New <see cref="ISqlSchemaBuilder"/> instance.</returns>
    /// <exception cref="SqlObjectBuilderException">When schema could not be created.</exception>
    ISqlSchemaBuilder Create(string name);

    /// <summary>
    /// Creates a new schema builder or returns an existing schema builder.
    /// </summary>
    /// <param name="name">Name of the schema.</param>
    /// <returns>New <see cref="ISqlSchemaBuilder"/> instance or an existing schema builder.</returns>
    /// <exception cref="SqlObjectBuilderException">When schema does not exist and could not be created.</exception>
    ISqlSchemaBuilder GetOrCreate(string name);

    /// <summary>
    /// Attempts to remove a schema by its <paramref name="name"/>.
    /// </summary>
    /// <param name="name">Name of the schema to remove.</param>
    /// <returns><b>true</b> when schema was removed, otherwise <b>false</b>.</returns>
    bool Remove(string name);
}
