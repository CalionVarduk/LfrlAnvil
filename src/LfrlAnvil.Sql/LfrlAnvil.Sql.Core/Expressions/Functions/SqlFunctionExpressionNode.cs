using System;
using System.Text;

namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlFunctionExpressionNode : SqlExpressionNode
{
    protected SqlFunctionExpressionNode(SqlFunctionType functionType, SqlExpressionNode[] arguments)
        : base( SqlNodeType.FunctionExpression )
    {
        FunctionType = functionType;
        Arguments = arguments;
    }

    public ReadOnlyMemory<SqlExpressionNode> Arguments { get; }
    public SqlFunctionType FunctionType { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        ArgumentsToString( builder.Append( FunctionType.ToString().ToUpperInvariant() ), indent );
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
}
