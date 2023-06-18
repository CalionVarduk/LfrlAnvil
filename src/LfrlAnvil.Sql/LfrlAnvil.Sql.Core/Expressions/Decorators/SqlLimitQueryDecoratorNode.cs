using System.Text;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlLimitQueryDecoratorNode : SqlQueryDecoratorNode
{
    internal SqlLimitQueryDecoratorNode(SqlExpressionNode value)
        : base( SqlNodeType.LimitDecorator )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "LIMIT" ).Append( ' ' ), Value, indent );
    }
}
