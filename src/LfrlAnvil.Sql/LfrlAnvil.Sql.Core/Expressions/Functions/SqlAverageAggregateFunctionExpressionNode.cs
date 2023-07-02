using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlAverageAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlAverageAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionDecoratorNode> decorators)
        : base( SqlFunctionType.Average, arguments, decorators )
    {
        var argumentType = arguments.Span[0].Type;
        Type = argumentType is null ? null : SqlExpressionType.Create<double>( isNullable: argumentType.Value.IsNullable );
    }

    public override SqlExpressionType? Type { get; }

    [Pure]
    public override SqlAverageAggregateFunctionExpressionNode Decorate(SqlAggregateFunctionDecoratorNode decorator)
    {
        var decorators = Decorators.ToExtendable().Extend( decorator );
        return new SqlAverageAggregateFunctionExpressionNode( Arguments, decorators );
    }
}
