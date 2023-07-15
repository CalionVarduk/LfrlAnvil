using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlSumAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlSumAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionTraitNode> traits)
        : base( SqlFunctionType.Sum, arguments, traits ) { }

    [Pure]
    public override SqlSumAggregateFunctionExpressionNode AddTrait(SqlAggregateFunctionTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlSumAggregateFunctionExpressionNode( Arguments, traits );
    }
}
