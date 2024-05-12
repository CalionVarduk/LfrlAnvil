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
