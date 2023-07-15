using System;
using System.Diagnostics.Contracts;
using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlAggregateFunctionExpressionNode : SqlExpressionNode
{
    protected SqlAggregateFunctionExpressionNode(
        SqlFunctionType functionType,
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionTraitNode> traits)
        : base( SqlNodeType.AggregateFunctionExpression )
    {
        FunctionType = functionType;
        Arguments = arguments;
        Traits = traits;
    }

    public SqlFunctionType FunctionType { get; }
    public ReadOnlyMemory<SqlExpressionNode> Arguments { get; }
    public Chain<SqlAggregateFunctionTraitNode> Traits { get; }

    [Pure]
    public abstract SqlAggregateFunctionExpressionNode AddTrait(SqlAggregateFunctionTraitNode trait);

    protected override void ToString(StringBuilder builder, int indent)
    {
        ArgumentsToString( builder.Append( "AGG_" ).Append( FunctionType.ToString().ToUpperInvariant() ), indent );
        TraitsToString( builder, indent );
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

    protected void TraitsToString(StringBuilder builder, int indent)
    {
        var traitIndent = indent + DefaultIndent;
        foreach ( var trait in Traits )
            AppendTo( builder.Indent( traitIndent ), trait, traitIndent );
    }
}
