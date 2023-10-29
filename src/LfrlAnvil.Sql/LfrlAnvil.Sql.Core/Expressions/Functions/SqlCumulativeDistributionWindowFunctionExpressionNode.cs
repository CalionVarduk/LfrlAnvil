using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCumulativeDistributionWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlCumulativeDistributionWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.CumulativeDistribution, ReadOnlyMemory<SqlExpressionNode>.Empty, traits ) { }

    [Pure]
    public override SqlCumulativeDistributionWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlCumulativeDistributionWindowFunctionExpressionNode( traits );
    }
}
