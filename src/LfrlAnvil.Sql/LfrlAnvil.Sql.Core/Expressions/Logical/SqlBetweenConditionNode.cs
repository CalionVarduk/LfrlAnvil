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

namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical between comparison.
/// </summary>
public sealed class SqlBetweenConditionNode : SqlConditionNode
{
    internal SqlBetweenConditionNode(SqlExpressionNode value, SqlExpressionNode min, SqlExpressionNode max, bool isNegated)
        : base( SqlNodeType.Between )
    {
        Value = value;
        Min = min;
        Max = max;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Minimum acceptable value.
    /// </summary>
    public SqlExpressionNode Min { get; }

    /// <summary>
    /// Maximum acceptable value.
    /// </summary>
    public SqlExpressionNode Max { get; }

    /// <summary>
    /// Specifies whether or not this node represents a not between comparison.
    /// </summary>
    public bool IsNegated { get; }
}
