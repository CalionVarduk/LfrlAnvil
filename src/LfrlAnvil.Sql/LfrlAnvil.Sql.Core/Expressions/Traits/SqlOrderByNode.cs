using System.Text;

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

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder, Expression, indent );
        builder.Append( ' ' ).Append( Ordering.Name );
    }
}
