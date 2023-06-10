using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlLimitDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlLimitDataSourceDecoratorNode(TDataSourceNode dataSource, SqlExpressionNode value)
        : base( SqlNodeType.LimitDecorator, dataSource )
    {
        Value = value;
    }

    internal SqlLimitDataSourceDecoratorNode(SqlDataSourceDecoratorNode<TDataSourceNode> @base, SqlExpressionNode value)
        : base( SqlNodeType.LimitDecorator, @base )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "LIMIT" ).Append( ' ' ), Value, indent );
    }
}
