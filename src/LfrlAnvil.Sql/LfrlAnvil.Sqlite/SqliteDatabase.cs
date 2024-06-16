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
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sql.Statements;
using LfrlAnvil.Sql.Versioning;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteDatabase : SqlDatabase
{
    internal SqliteDatabase(
        SqliteDatabaseBuilder builder,
        ISqliteDatabaseConnector connector,
        Version version,
        SqlQueryReaderExecutor<SqlDatabaseVersionRecord> versionRecordsQuery)
        : base(
            builder,
            new SqliteSchemaCollection( builder.Schemas ),
            ReinterpretCast.To<ISqlDatabaseConnector<DbConnection>>( connector ),
            version,
            versionRecordsQuery ) { }

    /// <inheritdoc cref="SqlDatabase.Schemas" />
    public new SqliteSchemaCollection Schemas => ReinterpretCast.To<SqliteSchemaCollection>( base.Schemas );

    /// <inheritdoc cref="SqlDatabase.DataTypes" />
    public new SqliteDataTypeProvider DataTypes => ReinterpretCast.To<SqliteDataTypeProvider>( base.DataTypes );

    /// <inheritdoc cref="SqlDatabase.TypeDefinitions" />
    public new SqliteColumnTypeDefinitionProvider TypeDefinitions =>
        ReinterpretCast.To<SqliteColumnTypeDefinitionProvider>( base.TypeDefinitions );

    /// <inheritdoc cref="SqlDatabase.NodeInterpreters" />
    public new SqliteNodeInterpreterFactory NodeInterpreters => ReinterpretCast.To<SqliteNodeInterpreterFactory>( base.NodeInterpreters );

    /// <inheritdoc cref="SqlDatabase.QueryReaders" />
    public new SqliteQueryReaderFactory QueryReaders => ReinterpretCast.To<SqliteQueryReaderFactory>( base.QueryReaders );

    /// <inheritdoc cref="SqlDatabase.ParameterBinders" />
    public new SqliteParameterBinderFactory ParameterBinders => ReinterpretCast.To<SqliteParameterBinderFactory>( base.ParameterBinders );

    /// <inheritdoc cref="SqlDatabase.Connector" />
    public new ISqliteDatabaseConnector Connector => ReinterpretCast.To<ISqliteDatabaseConnector>( base.Connector );

    /// <inheritdoc />
    public override void Dispose()
    {
        base.Dispose();
        if ( Connector is SqliteDatabasePermanentConnector permanentConnector )
            permanentConnector.CloseConnection();
    }
}
