﻿// Copyright 2024 Łukasz Furlepa
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

namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical check if <see cref="Value"/> exists in a set of values.
/// </summary>
public sealed class SqlInConditionNode : SqlConditionNode
{
    internal SqlInConditionNode(SqlExpressionNode value, SqlExpressionNode[] expressions, bool isNegated)
        : base( SqlNodeType.In )
    {
        Assume.IsNotEmpty( expressions );
        Value = value;
        Expressions = expressions;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Collection of values that the <see cref="Value"/> is compared against.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Expressions { get; }

    /// <summary>
    /// Specifies whether or not this node represents a check if <see cref="Value"/> does not exist in a set of values.
    /// </summary>
    public bool IsNegated { get; }
}
