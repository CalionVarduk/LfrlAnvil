using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlRankWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlRankWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Rank, ReadOnlyMemory<SqlExpressionNode>.Empty, traits ) { }

    [Pure]
    public override SqlRankWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlRankWindowFunctionExpressionNode( traits );
    }
}
