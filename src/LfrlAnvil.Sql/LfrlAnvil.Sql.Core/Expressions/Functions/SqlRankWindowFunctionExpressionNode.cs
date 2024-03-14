using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlRankWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlRankWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Rank, ReadOnlyArray<SqlExpressionNode>.Empty, traits ) { }

    [Pure]
    public override SqlRankWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlRankWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlRankWindowFunctionExpressionNode( traits );
    }
}
