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
using LfrlAnvil.MySql.Objects;
using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;

namespace LfrlAnvil.MySql;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlDatabase : SqlDatabase
{
    internal MySqlDatabase(
        MySqlDatabaseBuilder builder,
        MySqlDatabaseConnector connector,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base( builder, new MySqlSchemaCollection( builder.Schemas ), connector, version, versionRecordsQuery )
    {
        connector.SetDatabase( this );
    }

    /// <inheritdoc cref="SqlDatabase.Schemas" />
    public new MySqlSchemaCollection Schemas => ReinterpretCast.To<MySqlSchemaCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabase.DataTypes" />
    public new MySqlDataTypeProvider DataTypes => ReinterpretCast.To<MySqlDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabase.TypeDefinitions" />
    public new MySqlColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<MySqlColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabase.NodeInterpreters" />
    public new MySqlNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<MySqlNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabase.QueryReaders" />
    public new MySqlQueryReaderFactory QueryReaders => ReinterpretCast.To<MySqlQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabase.ParameterBinders" />
    public new MySqlParameterBinderFactory ParameterBinders => ReinterpretCast.To<MySqlParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabase.Connector" />
    public new MySqlDatabaseConnector Connector => ReinterpretCast.To<MySqlDatabaseConnector>( base.Connector );
}
