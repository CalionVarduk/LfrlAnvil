using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlFirstValueWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlFirstValueWindowFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.FirstValue, arguments, traits ) { }

    [Pure]
    public override SqlFirstValueWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlFirstValueWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlFirstValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
