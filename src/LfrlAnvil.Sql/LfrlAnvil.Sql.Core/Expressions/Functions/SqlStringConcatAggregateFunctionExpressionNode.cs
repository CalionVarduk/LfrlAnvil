using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlStringConcatAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlStringConcatAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlFunctionType.StringConcat, arguments, decorators ) { }

    [Pure]
    public override SqlStringConcatAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlStringConcatAggregateFunctionExpressionNode( Arguments, decorators );
    }
}
