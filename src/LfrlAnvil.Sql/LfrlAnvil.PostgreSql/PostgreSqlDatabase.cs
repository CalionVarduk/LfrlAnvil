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
using LfrlAnvil.PostgreSql.Objects;
using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlDatabase : SqlDatabase
{
    internal PostgreSqlDatabase(
        PostgreSqlDatabaseBuilder builder,
        PostgreSqlDatabaseConnector connector,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base( builder, new PostgreSqlSchemaCollection( builder.Schemas ), connector, version, versionRecordsQuery )
    {
        connector.SetDatabase( this );
    }

    /// <inheritdoc cref="SqlDatabase.Schemas" />
    public new PostgreSqlSchemaCollection Schemas => ReinterpretCast.To<PostgreSqlSchemaCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabase.DataTypes" />
    public new PostgreSqlDataTypeProvider DataTypes => ReinterpretCast.To<PostgreSqlDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabase.TypeDefinitions" />
    public new PostgreSqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<PostgreSqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabase.NodeInterpreters" />
    public new PostgreSqlNodeInterpreterFactory NodeInterpreters =>
        ReinterpretCast.To<PostgreSqlNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabase.QueryReaders" />
    public new PostgreSqlQueryReaderFactory QueryReaders => ReinterpretCast.To<PostgreSqlQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabase.ParameterBinders" />
    public new PostgreSqlParameterBinderFactory ParameterBinders =>
        ReinterpretCast.To<PostgreSqlParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabase.Connector" />
    public new PostgreSqlDatabaseConnector Connector => ReinterpretCast.To<PostgreSqlDatabaseConnector>( base.Connector );
}
