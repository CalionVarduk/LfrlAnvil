using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlNthValueWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlNthValueWindowFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.NthValue, arguments, traits ) { }

    [Pure]
    public override SqlNthValueWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlNthValueWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlNthValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
