using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSumAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlSumAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlFunctionType.Sum, arguments, decorators )
    {
        Type = arguments.Span[0].Type;
    }

    public override SqlExpressionType? Type { get; }

    [Pure]
    public override SqlSumAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlSumAggregateFunctionExpressionNode( Arguments, decorators );
    }
}
