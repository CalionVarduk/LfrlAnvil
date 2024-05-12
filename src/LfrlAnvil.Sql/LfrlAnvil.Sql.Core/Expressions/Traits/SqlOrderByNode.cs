namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single order by definition.
/// </summary>
public sealed class SqlOrderByNode : SqlNodeBase
{
    internal SqlOrderByNode(SqlExpressionNode expression, OrderBy ordering)
        : base( SqlNodeType.OrderBy )
    {
        Expression = expression;
        Ordering = ordering;
    }

    /// <summary>
    /// Underlying expression.
    /// </summary>
    public SqlExpressionNode Expression { get; }

    /// <summary>
    /// Ordering used by this definition.
    /// </summary>
    public OrderBy Ordering { get; }
}
