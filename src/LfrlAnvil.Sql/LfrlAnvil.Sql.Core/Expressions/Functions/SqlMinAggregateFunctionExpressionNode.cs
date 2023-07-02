using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMinAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlMinAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlFunctionType.Min, arguments, decorators )
    {
        Type = arguments.Span[0].Type;
    }

    public override SqlExpressionType? Type { get; }

    [Pure]
    public override SqlMinAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlMinAggregateFunctionExpressionNode( Arguments, decorators );
    }
}
