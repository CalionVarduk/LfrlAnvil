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

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single order by definition.
/// </summary>
public sealed class SqlOrderByNode : SqlNodeBase
{
    internal SqlOrderByNode(SqlExpressionNode expression, OrderBy ordering)
        : base( SqlNodeType.OrderBy )
    {
        Expression = expression;
        Ordering = ordering;
    }

    /// <summary>
    /// Underlying expression.
    /// </summary>
    public SqlExpressionNode Expression { get; }

    /// <summary>
    /// Ordering used by this definition.
    /// </summary>
    public OrderBy Ordering { get; }
}
