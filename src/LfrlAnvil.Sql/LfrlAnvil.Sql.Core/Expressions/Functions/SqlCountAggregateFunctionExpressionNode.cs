using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlCountAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlCountAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionTraitNode> traits)
        : base( SqlFunctionType.Count, arguments, traits ) { }

    [Pure]
    public override SqlCountAggregateFunctionExpressionNode AddTrait(SqlAggregateFunctionTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlCountAggregateFunctionExpressionNode( Arguments, traits );
    }
}
