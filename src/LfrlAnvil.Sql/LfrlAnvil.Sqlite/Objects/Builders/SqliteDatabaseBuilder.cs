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

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteDatabaseBuilder : SqlDatabaseBuilder
{
    internal SqliteDatabaseBuilder(
        string serverVersion,
        string defaultSchemaName,
        SqlDefaultObjectNameProvider defaultNames,
        SqliteDataTypeProvider dataTypes,
        SqliteColumnTypeDefinitionProvider typeDefinitions,
        SqliteNodeInterpreterFactory nodeInterpreters)
        : base(
            SqliteDialect.Instance,
            serverVersion,
            defaultSchemaName,
            dataTypes,
            typeDefinitions,
            nodeInterpreters,
            new SqliteQueryReaderFactory( typeDefinitions ),
            new SqliteParameterBinderFactory( typeDefinitions, nodeInterpreters.Options.ArePositionalParametersEnabled ),
            defaultNames,
            new SqliteSchemaBuilderCollection(),
            new SqliteDatabaseChangeTracker() ) { }

    /// <inheritdoc cref="SqlDatabaseBuilder.Schemas" />
    public new SqliteSchemaBuilderCollection Schemas => ReinterpretCast.To<SqliteSchemaBuilderCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabaseBuilder.DataTypes" />
    public new SqliteDataTypeProvider DataTypes => ReinterpretCast.To<SqliteDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabaseBuilder.TypeDefinitions" />
    public new SqliteColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<SqliteColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabaseBuilder.NodeInterpreters" />
    public new SqliteNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<SqliteNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabaseBuilder.QueryReaders" />
    public new SqliteQueryReaderFactory QueryReaders => ReinterpretCast.To<SqliteQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabaseBuilder.ParameterBinders" />
    public new SqliteParameterBinderFactory ParameterBinders => ReinterpretCast.To<SqliteParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabaseBuilder.Changes" />
    public new SqliteDatabaseChangeTracker Changes => ReinterpretCast.To<SqliteDatabaseChangeTracker>( base.Changes );

    /// <inheritdoc cref="SqlDatabaseBuilder.AddConnectionChangeCallback(Action{SqlDatabaseConnectionChangeEvent})" />
    public new SqliteDatabaseBuilder AddConnectionChangeCallback(Action<SqlDatabaseConnectionChangeEvent> callback)
    {
        base.AddConnectionChangeCallback( callback );
        return this;
    }

    /// <inheritdoc />
    [Pure]
    public override bool IsValidName(SqlObjectType objectType, string name)
    {
        if ( objectType == SqlObjectType.Schema && name.Length == 0 )
            return true;

        return base.IsValidName( objectType, name ) && ! name.Contains( '"' );
    }
}
