using System.Text;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlDistinctDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlDistinctDataSourceDecoratorNode(TDataSourceNode dataSource)
        : base( SqlNodeType.DistinctDecorator, dataSource ) { }

    internal SqlDistinctDataSourceDecoratorNode(SqlDataSourceDecoratorNode<TDataSourceNode> @base)
        : base( SqlNodeType.DistinctDecorator, @base ) { }

    protected override void ToString(StringBuilder builder, int indent)
    {
        builder.Append( "DISTINCT" );
    }
}
