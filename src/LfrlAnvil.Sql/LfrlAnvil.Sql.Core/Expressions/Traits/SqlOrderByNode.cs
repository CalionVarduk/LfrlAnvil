namespace LfrlAnvil.Sql.Expressions.Traits;

public sealed class SqlOrderByNode : SqlNodeBase
{
    internal SqlOrderByNode(SqlExpressionNode expression, OrderBy ordering)
        : base( SqlNodeType.OrderBy )
    {
        Expression = expression;
        Ordering = ordering;
    }

    public SqlExpressionNode Expression { get; }
    public OrderBy Ordering { get; }
}
