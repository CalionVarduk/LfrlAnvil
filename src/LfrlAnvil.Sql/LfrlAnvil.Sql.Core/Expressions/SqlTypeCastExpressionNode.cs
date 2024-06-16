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

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a type cast expression.
/// </summary>
public sealed class SqlTypeCastExpressionNode : SqlExpressionNode
{
    internal SqlTypeCastExpressionNode(SqlExpressionNode value, Type targetType)
        : base( SqlNodeType.TypeCast )
    {
        Value = value;
        TargetType = targetType;
        TargetTypeDefinition = null;
    }

    internal SqlTypeCastExpressionNode(SqlExpressionNode value, ISqlColumnTypeDefinition targetTypeDefinition)
        : base( SqlNodeType.TypeCast )
    {
        Value = value;
        TargetType = targetTypeDefinition.RuntimeType;
        TargetTypeDefinition = targetTypeDefinition;
    }

    /// <summary>
    /// Underlying value to cast to a different type.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Target runtime type.
    /// </summary>
    public Type TargetType { get; }

    /// <summary>
    /// Optional <see cref="ISqlColumnTypeDefinition"/> instance that defines the target type.
    /// </summary>
    public ISqlColumnTypeDefinition? TargetTypeDefinition { get; }
}
