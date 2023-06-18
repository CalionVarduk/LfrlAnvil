﻿using System;
using System.Text;
using LfrlAnvil.Extensions;

namespace LfrlAnvil.Sql.Expressions.Decorators;

public sealed class SqlAggregationDataSourceDecoratorNode : SqlDataSourceDecoratorNode
{
    internal SqlAggregationDataSourceDecoratorNode(SqlExpressionNode[] expressions)
        : base( SqlNodeType.AggregationDecorator )
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
