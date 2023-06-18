using System.Text;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlOffsetQueryDecoratorNode : SqlQueryDecoratorNode
{
    internal SqlOffsetQueryDecoratorNode(SqlExpressionNode value)
        : base( SqlNodeType.OffsetDecorator )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "OFFSET" ).Append( ' ' ), Value, indent );
    }
}
