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

using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

/// <summary>
/// Represents a collection of traits attached to an <see cref="SqlAggregateFunctionExpressionNode"/>.
/// </summary>
/// <param name="Distinct"><see cref="SqlDistinctTraitNode"/> instance.</param>
/// <param name="Filter">Predicate that is the result of parsing of all <see cref="SqlFilterTraitNode"/> instances.</param>
/// <param name="Window"><see cref="SqlWindowDefinitionNode"/> instance.</param>
/// <param name="Ordering">
/// Collection of ordering expressions that is the result of parsing of all <see cref="SqlSortTraitNode"/> instances.
/// </param>
/// <param name="Custom">Collection of all unrecognized <see cref="SqlTraitNode"/> instances.</param>
public readonly record struct SqlAggregateFunctionTraits(
    SqlDistinctTraitNode? Distinct,
    SqlConditionNode? Filter,
    SqlWindowDefinitionNode? Window,
    Chain<ReadOnlyArray<SqlOrderByNode>> Ordering,
    Chain<SqlTraitNode> Custom
);
