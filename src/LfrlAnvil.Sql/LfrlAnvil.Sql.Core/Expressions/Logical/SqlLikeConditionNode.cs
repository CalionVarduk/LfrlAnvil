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
/// Represents an SQL syntax tree condition node that defines a logical check if <see cref="Value"/>
/// satisfies a string <see cref="Pattern"/>.
/// </summary>
public sealed class SqlLikeConditionNode : SqlConditionNode
{
    internal SqlLikeConditionNode(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape, bool isNegated)
        : base( SqlNodeType.Like )
    {
        Value = value;
        Pattern = pattern;
        Escape = escape;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// String pattern to check the <see cref="Value"/> against.
    /// </summary>
    public SqlExpressionNode Pattern { get; }

    /// <summary>
    /// Optional escape character for the <see cref="Pattern"/>.
    /// </summary>
    public SqlExpressionNode? Escape { get; }

    /// <summary>
    /// Specifies whether or not this node represents a check if <see cref="Value"/> does not satisfy a string <see cref="Pattern"/>.
    /// </summary>
    public bool IsNegated { get; }
}
