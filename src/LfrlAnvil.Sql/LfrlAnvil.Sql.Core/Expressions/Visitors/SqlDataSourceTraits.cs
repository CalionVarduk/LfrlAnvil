using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlDataSourceTraits(
    Chain<ReadOnlyArray<SqlCommonTableExpressionNode>> CommonTableExpressions,
    bool ContainsRecursiveCommonTableExpression,
    SqlDistinctTraitNode? Distinct,
    SqlConditionNode? Filter,
    Chain<ReadOnlyArray<SqlExpressionNode>> Aggregations,
    SqlConditionNode? AggregationFilter,
    Chain<ReadOnlyArray<SqlWindowDefinitionNode>> Windows,
    Chain<ReadOnlyArray<SqlOrderByNode>> Ordering,
    SqlExpressionNode? Limit,
    SqlExpressionNode? Offset,
    Chain<SqlTraitNode> Custom);
