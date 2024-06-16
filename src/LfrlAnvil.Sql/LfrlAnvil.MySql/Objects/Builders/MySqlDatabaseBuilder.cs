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
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlDatabaseBuilder : SqlDatabaseBuilder
{
    internal MySqlDatabaseBuilder(
        string serverVersion,
        string defaultSchemaName,
        SqlDefaultObjectNameProvider defaultNames,
        MySqlDataTypeProvider dataTypes,
        MySqlColumnTypeDefinitionProvider typeDefinitions,
        MySqlNodeInterpreterFactory nodeInterpreters,
        SqlOptionalFunctionalityResolution indexFilterResolution,
        string? characterSetName,
        string? collationName,
        bool? isEncryptionEnabled)
        : base(
            MySqlDialect.Instance,
            serverVersion,
            defaultSchemaName,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            new MySqlQueryReaderFactory( typeDefinitions ),
            new MySqlParameterBinderFactory( typeDefinitions ),
            defaultNames,
            new MySqlSchemaBuilderCollection(),
            new MySqlDatabaseChangeTracker() )
    {
        Assume.IsDefined( indexFilterResolution );
        CommonSchemaName = defaultSchemaName;
        IndexFilterResolution = indexFilterResolution;
        CharacterSetName = characterSetName;
        CollationName = collationName;
        IsEncryptionEnabled = isEncryptionEnabled;
    }

    /// <summary>
    /// Name of the common schema.
    /// </summary>
    /// <remarks>
    /// This schema will contain common functions and procedures and it will not be possible to remove a schema with this name.
    /// </remarks>
    public string CommonSchemaName { get; }

    /// <summary>
    /// Specifies how partial indexes should be resolved.
    /// </summary>
    public SqlOptionalFunctionalityResolution IndexFilterResolution { get; }

    /// <summary>
    /// Specifies default DB character set.
    /// </summary>
    public string? CharacterSetName { get; }

    /// <summary>
    /// Specifies default DB collation.
    /// </summary>
    public string? CollationName { get; }

    /// <summary>
    /// Specifies whether or not DB encryption should be enabled.
    /// </summary>
    public bool? IsEncryptionEnabled { get; }

    /// <inheritdoc cref="SqlDatabaseBuilder.Schemas" />
    public new MySqlSchemaBuilderCollection Schemas => ReinterpretCast.To<MySqlSchemaBuilderCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabaseBuilder.DataTypes" />
    public new MySqlDataTypeProvider DataTypes => ReinterpretCast.To<MySqlDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabaseBuilder.TypeDefinitions" />
    public new MySqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<MySqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabaseBuilder.NodeInterpreters" />
    public new MySqlNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<MySqlNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabaseBuilder.QueryReaders" />
    public new MySqlQueryReaderFactory QueryReaders => ReinterpretCast.To<MySqlQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabaseBuilder.ParameterBinders" />
    public new MySqlParameterBinderFactory ParameterBinders => ReinterpretCast.To<MySqlParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabaseBuilder.Changes" />
    public new MySqlDatabaseChangeTracker Changes => ReinterpretCast.To<MySqlDatabaseChangeTracker>( base.Changes );

    /// <inheritdoc cref="SqlDatabaseBuilder.AddConnectionChangeCallback(Action{SqlDatabaseConnectionChangeEvent})" />
    public new MySqlDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public override bool IsValidName(SqlObjectType objectType, string name)
    {
        return base.IsValidName( objectType, name ) && ! name.Contains( '`' );
    }
}
