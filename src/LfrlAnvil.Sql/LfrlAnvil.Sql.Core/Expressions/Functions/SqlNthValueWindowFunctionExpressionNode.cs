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
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlNthValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
