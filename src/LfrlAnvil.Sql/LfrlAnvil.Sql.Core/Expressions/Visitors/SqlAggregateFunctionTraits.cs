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
