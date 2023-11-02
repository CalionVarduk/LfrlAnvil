using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlAverageAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlAverageAggregateFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Average, arguments, traits ) { }

    [Pure]
    public override SqlAverageAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlAverageAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlAverageAggregateFunctionExpressionNode( Arguments, traits );
    }
}
