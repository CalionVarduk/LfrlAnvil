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
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlMaxAggregateFunctionExpressionNode( Arguments, traits );
    }
}
