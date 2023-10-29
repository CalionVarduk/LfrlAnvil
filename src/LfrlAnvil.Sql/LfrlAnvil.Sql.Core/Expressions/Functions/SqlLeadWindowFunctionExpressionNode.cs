using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLeadWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlLeadWindowFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Lead, arguments, traits ) { }

    [Pure]
    public override SqlLeadWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlLeadWindowFunctionExpressionNode( Arguments, traits );
    }
}
