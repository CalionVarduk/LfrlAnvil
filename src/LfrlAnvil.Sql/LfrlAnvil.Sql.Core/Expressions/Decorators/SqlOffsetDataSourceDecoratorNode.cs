using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlOffsetDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlOffsetDataSourceDecoratorNode(TDataSourceNode dataSource, SqlExpressionNode value)
        : base( SqlNodeType.OffsetDecorator, dataSource )
    {
        Value = value;
    }

    internal SqlOffsetDataSourceDecoratorNode(SqlDataSourceDecoratorNode<TDataSourceNode> @base, SqlExpressionNode value)
        : base( SqlNodeType.OffsetDecorator, @base )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "OFFSET" ).Append( ' ' ), Value, indent );
    }
}
