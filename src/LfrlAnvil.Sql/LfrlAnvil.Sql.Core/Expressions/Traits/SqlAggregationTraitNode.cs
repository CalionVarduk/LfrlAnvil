namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlAggregationTraitNode : SqlTraitNode
{
    internal SqlAggregationTraitNode(SqlExpressionNode[] expressions)
        : base( SqlNodeType.AggregationTrait )
    {
        Expressions = expressions;
    }

    public ReadOnlyArray<SqlExpressionNode> Expressions { get; }
}
