namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single aggregation trait.
/// </summary>
public sealed class SqlAggregationTraitNode : SqlTraitNode
{
    internal SqlAggregationTraitNode(SqlExpressionNode[] expressions)
        : base( SqlNodeType.AggregationTrait )
    {
        Expressions = expressions;
    }

    /// <summary>
    /// Collection of expressions to aggregate by.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Expressions { get; }
}
