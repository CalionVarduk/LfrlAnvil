using LfrlAnvil.Sql.Expressions.Logical;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Visitors;

public readonly record struct SqlAggregateFunctionTraits(
    SqlDistinctTraitNode? Distinct,
    SqlConditionNode? Filter,
    Chain<SqlTraitNode> Custom);
