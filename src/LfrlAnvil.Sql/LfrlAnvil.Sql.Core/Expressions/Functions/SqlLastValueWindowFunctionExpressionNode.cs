using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlLastValueWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlLastValueWindowFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.LastValue, arguments, traits ) { }

    [Pure]
    public override SqlLastValueWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlLastValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
