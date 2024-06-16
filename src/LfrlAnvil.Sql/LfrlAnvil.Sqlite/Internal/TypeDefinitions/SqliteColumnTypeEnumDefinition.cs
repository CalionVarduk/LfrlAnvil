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
using LfrlAnvil.Sql;
using Microsoft.Data.Sqlite;

namespace LfrlAnvil.Sqlite.Internal.TypeDefinitions;

/// <summary>
/// Represents a generic definition of an <see cref="Enum"/> column type for <see cref="SqliteDialect"/>.
/// </summary>
/// <typeparam name="TEnum">Underlying .NET <see cref="Enum"/> type.</typeparam>
/// <typeparam name="TUnderlying">.NET type of the underlying value of <typeparamref name="TEnum"/> type.</typeparam>
public sealed class SqliteColumnTypeEnumDefinition<TEnum, TUnderlying>
    : SqlColumnTypeEnumDefinition<TEnum, TUnderlying, SqliteDataReader, SqliteParameter>
    where TEnum : struct, Enum
    where TUnderlying : unmanaged
{
    /// <summary>
    /// Creates a new <see cref="SqliteColumnTypeEnumDefinition{TEnum,TUnderlying}"/> instance.
    /// </summary>
    /// <param name="base">Column type definition associated with the underlying type.</param>
    public SqliteColumnTypeEnumDefinition(SqliteColumnTypeDefinition<TUnderlying> @base)
        : base( @base ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new SqliteDataType DataType => ReinterpretCast.To<SqliteDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(SqliteParameter parameter, bool isNullable)
    {
        base.SetParameterInfo( parameter, isNullable );
        parameter.SqliteType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
