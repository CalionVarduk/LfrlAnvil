using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMaxAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlMaxAggregateFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Max, arguments, traits ) { }

    [Pure]
    public override SqlMaxAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlMaxAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlMaxAggregateFunctionExpressionNode( Arguments, traits );
    }
}
