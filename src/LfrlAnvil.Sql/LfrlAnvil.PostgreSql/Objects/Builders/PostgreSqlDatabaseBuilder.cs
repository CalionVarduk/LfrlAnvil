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
using LfrlAnvil.PostgreSql.Internal;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.PostgreSql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlDatabaseBuilder : SqlDatabaseBuilder
{
    internal PostgreSqlDatabaseBuilder(
        string serverVersion,
        string defaultSchemaName,
        SqlDefaultObjectNameProvider defaultNames,
        PostgreSqlDataTypeProvider dataTypes,
        PostgreSqlColumnTypeDefinitionProvider typeDefinitions,
        PostgreSqlNodeInterpreterFactory nodeInterpreters,
        SqlOptionalFunctionalityResolution virtualGeneratedColumnStorageResolution)
        : base(
            PostgreSqlDialect.Instance,
            serverVersion,
            defaultSchemaName,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            new PostgreSqlQueryReaderFactory( typeDefinitions ),
            new PostgreSqlParameterBinderFactory( typeDefinitions ),
            defaultNames,
            new PostgreSqlSchemaBuilderCollection(),
            new PostgreSqlDatabaseChangeTracker() )
    {
        Assume.IsDefined( virtualGeneratedColumnStorageResolution );
        VirtualGeneratedColumnStorageResolution = virtualGeneratedColumnStorageResolution;

        if ( ! defaultSchemaName.Equals( PostgreSqlHelpers.DefaultVersionHistoryName.Schema ) )
            Schemas.Create( PostgreSqlHelpers.DefaultVersionHistoryName.Schema );
    }

    /// <summary>
    /// Specifies how virtual computed columns should be resolved.
    /// </summary>
    public SqlOptionalFunctionalityResolution VirtualGeneratedColumnStorageResolution { get; }

    /// <inheritdoc cref="SqlDatabaseBuilder.Schemas" />
    public new PostgreSqlSchemaBuilderCollection Schemas => ReinterpretCast.To<PostgreSqlSchemaBuilderCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabaseBuilder.DataTypes" />
    public new PostgreSqlDataTypeProvider DataTypes => ReinterpretCast.To<PostgreSqlDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabaseBuilder.TypeDefinitions" />
    public new PostgreSqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<PostgreSqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabaseBuilder.NodeInterpreters" />
    public new PostgreSqlNodeInterpreterFactory NodeInterpreters =>
        ReinterpretCast.To<PostgreSqlNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabaseBuilder.QueryReaders" />
    public new PostgreSqlQueryReaderFactory QueryReaders => ReinterpretCast.To<PostgreSqlQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabaseBuilder.ParameterBinders" />
    public new PostgreSqlParameterBinderFactory ParameterBinders =>
        ReinterpretCast.To<PostgreSqlParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabaseBuilder.Changes" />
    public new PostgreSqlDatabaseChangeTracker Changes => ReinterpretCast.To<PostgreSqlDatabaseChangeTracker>( base.Changes );

    /// <inheritdoc cref="SqlDatabaseBuilder.AddConnectionChangeCallback(Action{SqlDatabaseConnectionChangeEvent})" />
    public new PostgreSqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public override bool IsValidName(SqlObjectType objectType, string name)
    {
        return base.IsValidName( objectType, name ) && ! name.Contains( '"' );
    }
}
