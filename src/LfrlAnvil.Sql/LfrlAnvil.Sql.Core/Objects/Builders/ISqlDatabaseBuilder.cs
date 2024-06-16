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
using System.Data.Common;
using LfrlAnvil.Sql.Expressions.Visitors;
using LfrlAnvil.Sql.Statements.Compilers;

namespace LfrlAnvil.Sql.Objects.Builders;

/// <summary>
/// Represents an SQL database builder.
/// </summary>
public interface ISqlDatabaseBuilder
{
    /// <summary>
    /// Specifies the SQL dialect of this database.
    /// </summary>
    SqlDialect Dialect { get; }

    /// <summary>
    /// Provider of SQL data types.
    /// </summary>
    ISqlDataTypeProvider DataTypes { get; }

    /// <summary>
    /// Provider of column type definitions.
    /// </summary>
    ISqlColumnTypeDefinitionProvider TypeDefinitions { get; }

    /// <summary>
    /// Factory of node interpreters.
    /// </summary>
    ISqlNodeInterpreterFactory NodeInterpreters { get; }

    /// <summary>
    /// Factory of query readers.
    /// </summary>
    ISqlQueryReaderFactory QueryReaders { get; }

    /// <summary>
    /// Factory of parameter binders.
    /// </summary>
    ISqlParameterBinderFactory ParameterBinders { get; }

    /// <summary>
    /// Provider of default SQL object names.
    /// </summary>
    ISqlDefaultObjectNameProvider DefaultNames { get; }

    /// <summary>
    /// Collection of schemas defined in this database.
    /// </summary>
    ISqlSchemaBuilderCollection Schemas { get; }

    /// <summary>
    /// Tracker of changes applied to this database.
    /// </summary>
    ISqlDatabaseChangeTracker Changes { get; }

    /// <summary>
    /// Current <see cref="DbConnection.ServerVersion"/> of this database.
    /// </summary>
    string ServerVersion { get; }

    /// <summary>
    /// Adds an <see cref="SqlDatabaseConnectionChangeEvent"/> callback.
    /// </summary>
    /// <param name="callback">Callback to add.</param>
    /// <returns><b>this</b>.</returns>
    ISqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback);
}
