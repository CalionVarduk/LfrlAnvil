using System;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlAggregationDataSourceDecoratorNode<TDataSourceNode> : SqlDataSourceDecoratorNode<TDataSourceNode>
    where TDataSourceNode : SqlDataSourceNode
{
    internal SqlAggregationDataSourceDecoratorNode(TDataSourceNode dataSource, SqlExpressionNode[] expressions)
        : base( SqlNodeType.AggregationDecorator, dataSource )
    {
        Expressions = expressions;
    }

    internal SqlAggregationDataSourceDecoratorNode(SqlDataSourceDecoratorNode<TDataSourceNode> @base, SqlExpressionNode[] expressions)
        : base( SqlNodeType.AggregationDecorator, @base )
    {
        Expressions = expressions;
    }

    public ReadOnlyMemory<SqlExpressionNode> Expressions { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var expressionIndent = indent + DefaultIndent;
        builder.Append( "GROUP BY" );

        if ( Expressions.Length == 0 )
            return;

        foreach ( var expression in Expressions.Span )
        {
            AppendChildTo( builder.Indent( expressionIndent ), expression, expressionIndent );
            builder.Append( ',' );
        }

        builder.Length -= 1;
    }
}
