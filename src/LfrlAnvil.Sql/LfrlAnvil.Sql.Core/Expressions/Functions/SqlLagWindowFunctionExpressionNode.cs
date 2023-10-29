using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLagWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlLagWindowFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Lag, arguments, traits ) { }

    [Pure]
    public override SqlLagWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlLagWindowFunctionExpressionNode( Arguments, traits );
    }
}
