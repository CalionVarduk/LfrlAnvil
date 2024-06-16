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

using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a single data field of a record set
/// based on an <see cref="ISqlColumnBuilder"/> instance.
/// </summary>
public sealed class SqlColumnBuilderNode : SqlDataFieldNode
{
    private readonly bool _isOptional;

    internal SqlColumnBuilderNode(SqlRecordSetNode recordSet, ISqlColumnBuilder value, bool isOptional)
        : base( recordSet, SqlNodeType.ColumnBuilder )
    {
        Value = value;
        _isOptional = isOptional;
    }

    /// <summary>
    /// Underlying <see cref="ISqlColumnBuilder"/> instance.
    /// </summary>
    public ISqlColumnBuilder Value { get; }

    /// <summary>
    /// Runtime type of this data field.
    /// </summary>
    public TypeNullability Type => TypeNullability.Create( Value.TypeDefinition.RuntimeType, _isOptional || Value.IsNullable );

    /// <inheritdoc />
    public override string Name => Value.Name;

    /// <inheritdoc />
    [Pure]
    public override SqlColumnBuilderNode ReplaceRecordSet(SqlRecordSetNode recordSet)
    {
        return new SqlColumnBuilderNode( recordSet, Value, _isOptional );
    }
}
