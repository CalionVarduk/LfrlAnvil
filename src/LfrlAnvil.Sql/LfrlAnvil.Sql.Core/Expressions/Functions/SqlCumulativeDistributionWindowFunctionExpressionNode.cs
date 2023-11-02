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
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlCumulativeDistributionWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlCumulativeDistributionWindowFunctionExpressionNode( traits );
    }
}
