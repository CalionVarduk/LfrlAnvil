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

namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single aggregation filter trait.
/// </summary>
public sealed class SqlAggregationFilterTraitNode : SqlTraitNode
{
    internal SqlAggregationFilterTraitNode(SqlConditionNode filter, bool isConjunction)
        : base( SqlNodeType.AggregationFilterTrait )
    {
        Filter = filter;
        IsConjunction = isConjunction;
    }

    /// <summary>
    /// Underlying predicate.
    /// </summary>
    public SqlConditionNode Filter { get; }

    /// <summary>
    /// Specifies whether or not this trait should be merged with other <see cref="SqlAggregationFilterTraitNode"/> instances through
    /// an <see cref="SqlAndConditionNode"/> rather than an <see cref="SqlOrConditionNode"/>.
    /// </summary>
    public bool IsConjunction { get; }
}
