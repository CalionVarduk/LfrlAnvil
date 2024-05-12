using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function
/// that calculates the cumulative distribution.
/// </summary>
public sealed class SqlCumulativeDistributionWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlCumulativeDistributionWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.CumulativeDistribution, ReadOnlyArray<SqlExpressionNode>.Empty, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlCumulativeDistributionWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlCumulativeDistributionWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlCumulativeDistributionWindowFunctionExpressionNode( traits );
    }
}
