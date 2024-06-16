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
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql;

/// <inheritdoc cref="ISqlColumnTypeDefinitionProviderBuilder" />
public abstract class SqlColumnTypeDefinitionProviderBuilder : ISqlColumnTypeDefinitionProviderBuilder
{
    internal readonly Dictionary<Type, SqlColumnTypeDefinition> Definitions;

    /// <summary>
    /// Creates a new empty <see cref="SqlColumnTypeDefinitionProviderBuilder"/> instance.
    /// </summary>
    /// <param name="dialect">Specifies the SQL dialect of this builder.</param>
    protected SqlColumnTypeDefinitionProviderBuilder(SqlDialect dialect)
    {
        Definitions = new Dictionary<Type, SqlColumnTypeDefinition>();
        Dialect = dialect;
    }

    /// <inheritdoc />
    public SqlDialect Dialect { get; }

    /// <inheritdoc />
    [Pure]
    public bool Contains(Type type)
    {
        return Definitions.ContainsKey( type );
    }

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProviderBuilder.Register(ISqlColumnTypeDefinition)" />
    public SqlColumnTypeDefinitionProviderBuilder Register(SqlColumnTypeDefinition definition)
    {
        Ensure.Equals( definition.DataType.Dialect, Dialect );
        Definitions[definition.RuntimeType] = definition;
        return this;
    }

    /// <inheritdoc cref="ISqlColumnTypeDefinitionProviderBuilder.Build()" />
    [Pure]
    public abstract SqlColumnTypeDefinitionProvider Build();

    /// <summary>
    /// Adds or updates the provided column type <paramref name="definition"/>
    /// by its <see cref="ISqlColumnTypeDefinition.RuntimeType"/> to this builder.
    /// </summary>
    /// <param name="definition">Definition to register.</param>
    protected void AddOrUpdate(SqlColumnTypeDefinition definition)
    {
        Assume.Equals( definition.DataType.Dialect, Dialect );
        Definitions[definition.RuntimeType] = definition;
    }

    ISqlColumnTypeDefinitionProviderBuilder ISqlColumnTypeDefinitionProviderBuilder.Register(ISqlColumnTypeDefinition definition)
    {
        return Register( SqlHelpers.CastOrThrow<SqlColumnTypeDefinition>( Dialect, definition ) );
    }

    [Pure]
    ISqlColumnTypeDefinitionProvider ISqlColumnTypeDefinitionProviderBuilder.Build()
    {
        return Build();
    }
}
