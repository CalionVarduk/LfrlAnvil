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
using System.Linq.Expressions;
using LfrlAnvil.Sql;
using Npgsql;

namespace LfrlAnvil.PostgreSql;

/// <inheritdoc cref="SqlColumnTypeDefinition{T}" />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public abstract class PostgreSqlColumnTypeDefinition<T> : SqlColumnTypeDefinition<T, NpgsqlDataReader, NpgsqlParameter>
    where T : notnull
{
    /// <summary>
    /// Creates a new <see cref="PostgreSqlColumnTypeDefinition{T}"/> instance.
    /// </summary>
    /// <param name="dataType">Underlying DB data type.</param>
    /// <param name="defaultValue">Specifies the default value for this type.</param>
    /// <param name="outputMapping">
    /// Specifies the mapping of values read by <see cref="NpgsqlDataReader"/> to objects
    /// of the specified <see cref="ISqlColumnTypeDefinition.RuntimeType"/>.
    /// </param>
    protected PostgreSqlColumnTypeDefinition(
        PostgreSqlDataType dataType,
        T defaultValue,
        Expression<Func<NpgsqlDataReader, int, T>> outputMapping)
        : base( dataType, defaultValue, outputMapping ) { }

    /// <inheritdoc cref="SqlColumnTypeDefinition.DataType" />
    public new PostgreSqlDataType DataType => ReinterpretCast.To<PostgreSqlDataType>( base.DataType );

    /// <inheritdoc />
    public override void SetParameterInfo(NpgsqlParameter parameter, bool isNullable)
    {
        parameter.NpgsqlDbType = DataType.Value;
        parameter.IsNullable = isNullable;
    }
}
