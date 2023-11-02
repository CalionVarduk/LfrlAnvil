using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlDenseRankWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlDenseRankWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.DenseRank, ReadOnlyMemory<SqlExpressionNode>.Empty, traits ) { }

    [Pure]
    public override SqlDenseRankWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlDenseRankWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlDenseRankWindowFunctionExpressionNode( traits );
    }
}
