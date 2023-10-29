using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlNTileWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlNTileWindowFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.NTile, arguments, traits ) { }

    [Pure]
    public override SqlNTileWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlNTileWindowFunctionExpressionNode( Arguments, traits );
    }
}
