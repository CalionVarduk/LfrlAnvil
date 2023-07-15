using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlAverageAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlAverageAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionTraitNode> traits)
        : base( SqlFunctionType.Average, arguments, traits ) { }

    [Pure]
    public override SqlAverageAggregateFunctionExpressionNode AddTrait(SqlAggregateFunctionTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlAverageAggregateFunctionExpressionNode( Arguments, traits );
    }
}
