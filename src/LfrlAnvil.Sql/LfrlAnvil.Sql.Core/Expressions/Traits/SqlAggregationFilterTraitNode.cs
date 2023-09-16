using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlAggregationFilterTraitNode : SqlTraitNode
{
    internal SqlAggregationFilterTraitNode(SqlConditionNode filter, bool isConjunction)
        : base( SqlNodeType.AggregationFilterTrait )
    {
        Filter = filter;
        IsConjunction = isConjunction;
    }

    public SqlConditionNode Filter { get; }
    public bool IsConjunction { get; }
}
