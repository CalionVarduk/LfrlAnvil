using System;
using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlDataSourceTraits(
    Chain<ReadOnlyMemory<SqlCommonTableExpressionNode>> CommonTableExpressions,
    SqlDistinctTraitNode? Distinct,
    SqlConditionNode? Filter,
    Chain<ReadOnlyMemory<SqlExpressionNode>> Aggregations,
    SqlConditionNode? AggregationFilter,
    Chain<ReadOnlyMemory<SqlWindowDefinitionNode>> Windows,
    Chain<ReadOnlyMemory<SqlOrderByNode>> Ordering,
    SqlExpressionNode? Limit,
    SqlExpressionNode? Offset,
    Chain<SqlTraitNode> Custom);
