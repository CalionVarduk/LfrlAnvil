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
using System.Data;
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite;

/// <inheritdoc cref="ISqlDataType" />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteDataType : Enumeration<SqliteDataType, SqliteType>, ISqlDataType
{
    /// <summary>
    /// Represents the <b>INTEGER</b> type.
    /// </summary>
    public static readonly SqliteDataType Integer = new SqliteDataType( "INTEGER", SqliteType.Integer, DbType.Int64 );

    /// <summary>
    /// Represents the <b>REAL</b> type.
    /// </summary>
    public static readonly SqliteDataType Real = new SqliteDataType( "REAL", SqliteType.Real, DbType.Double );

    /// <summary>
    /// Represents the <b>TEXT</b> type.
    /// </summary>
    public static readonly SqliteDataType Text = new SqliteDataType( "TEXT", SqliteType.Text, DbType.String );

    /// <summary>
    /// Represents the <b>BLOB</b> type.
    /// </summary>
    public static readonly SqliteDataType Blob = new SqliteDataType( "BLOB", SqliteType.Blob, DbType.Binary );

    /// <summary>
    /// Represents the <b>ANY</b> type.
    /// </summary>
    public static readonly SqliteDataType Any = new SqliteDataType( "ANY", 0, DbType.Object );

    private SqliteDataType(string name, SqliteType value, DbType dbType)
        : base( name, value )
    {
        DbType = dbType;
    }

    /// <inheritdoc />
    public DbType DbType { get; }

    /// <inheritdoc />
    public SqlDialect Dialect => SqliteDialect.Instance;

    ReadOnlySpan<int> ISqlDataType.Parameters => ReadOnlySpan<int>.Empty;
    ReadOnlySpan<SqlDataTypeParameter> ISqlDataType.ParameterDefinitions => ReadOnlySpan<SqlDataTypeParameter>.Empty;
}
