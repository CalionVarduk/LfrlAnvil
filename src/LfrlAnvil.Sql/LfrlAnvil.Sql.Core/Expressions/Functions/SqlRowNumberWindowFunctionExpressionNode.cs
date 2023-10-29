using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlRowNumberWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlRowNumberWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.RowNumber, ReadOnlyMemory<SqlExpressionNode>.Empty, traits ) { }

    [Pure]
    public override SqlRowNumberWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlRowNumberWindowFunctionExpressionNode( traits );
    }
}
