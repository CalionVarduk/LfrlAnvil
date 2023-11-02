using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSumAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlSumAggregateFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Sum, arguments, traits ) { }

    [Pure]
    public override SqlSumAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlSumAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlSumAggregateFunctionExpressionNode( Arguments, traits );
    }
}
