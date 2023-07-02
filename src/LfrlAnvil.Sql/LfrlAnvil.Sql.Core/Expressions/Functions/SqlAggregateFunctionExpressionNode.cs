using System;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlAggregateFunctionExpressionNode : SqlExpressionNode
{
    protected SqlAggregateFunctionExpressionNode(
        SqlFunctionType functionType,
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlNodeType.AggregateFunctionExpression )
    {
        FunctionType = functionType;
        Arguments = arguments;
        Decorators = decorators;
    }

    public SqlFunctionType FunctionType { get; }
    public ReadOnlyMemory<SqlExpressionNode> Arguments { get; }
    public Chain<SqlAggregateFunctionDecoratorNode> Decorators { get; }

    [Pure]
    public abstract SqlAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator);

    protected override void ToString(StringBuilder builder, int indent)
    {
        ArgumentsToString( builder.Append( "AGG_" ).Append( FunctionType.ToString().ToUpperInvariant() ), indent );
        DecoratorsToString( builder, indent );
    }

    protected void ArgumentsToString(StringBuilder builder, int indent)
    {
        builder.Append( '(' );

        if ( Arguments.Length > 0 )
        {
            foreach ( var arg in Arguments.Span )
            {
                AppendChildTo( builder, arg, indent );
                builder.Append( ',' ).Append( ' ' );
            }

            builder.Length -= 2;
        }

        builder.Append( ')' );
    }

    protected void DecoratorsToString(StringBuilder builder, int indent)
    {
        var decoratorIndent = indent + DefaultIndent;
        foreach ( var decorator in Decorators )
            AppendTo( builder.Indent( decoratorIndent ), decorator, decoratorIndent );
    }
}
