using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlAggregateFunctionTraits(
    SqlDistinctTraitNode? Distinct,
    SqlConditionNode? Filter,
    SqlWindowDefinitionNode? Window,
    Chain<ReadOnlyArray<SqlOrderByNode>> Ordering,
    Chain<SqlTraitNode> Custom
);
