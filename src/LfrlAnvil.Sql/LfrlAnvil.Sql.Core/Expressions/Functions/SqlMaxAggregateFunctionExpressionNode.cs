using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMaxAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlMaxAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlFunctionType.Max, arguments, decorators )
    {
        Type = arguments.Span[0].Type;
    }

    public override SqlExpressionType? Type { get; }

    [Pure]
    public override SqlMaxAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlMaxAggregateFunctionExpressionNode( Arguments, decorators );
    }
}
