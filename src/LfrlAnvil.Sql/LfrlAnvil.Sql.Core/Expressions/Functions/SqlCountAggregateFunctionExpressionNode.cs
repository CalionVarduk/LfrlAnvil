using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCountAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlCountAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlFunctionType.Count, arguments, decorators )
    {
        Type = SqlExpressionType.Create<long>();
    }

    public override SqlExpressionType? Type { get; }

    [Pure]
    public override SqlCountAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlCountAggregateFunctionExpressionNode( Arguments, decorators );
    }
}
