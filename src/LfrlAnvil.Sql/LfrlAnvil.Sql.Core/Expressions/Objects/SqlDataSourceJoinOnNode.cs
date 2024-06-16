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

using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set join operation.
/// </summary>
public sealed class SqlDataSourceJoinOnNode : SqlNodeBase
{
    internal SqlDataSourceJoinOnNode(
        SqlJoinType joinType,
        SqlRecordSetNode innerRecordSet,
        SqlConditionNode onExpression)
        : base( SqlNodeType.JoinOn )
    {
        JoinType = joinType;
        InnerRecordSet = innerRecordSet;
        OnExpression = onExpression;
    }

    /// <summary>
    /// Type of this join operation.
    /// </summary>
    public SqlJoinType JoinType { get; }

    /// <summary>
    /// Inner <see cref="SqlRecordSetNode"/> instance.
    /// </summary>
    public SqlRecordSetNode InnerRecordSet { get; }

    /// <summary>
    /// Condition of this join operation.
    /// </summary>
    public SqlConditionNode OnExpression { get; }
}
