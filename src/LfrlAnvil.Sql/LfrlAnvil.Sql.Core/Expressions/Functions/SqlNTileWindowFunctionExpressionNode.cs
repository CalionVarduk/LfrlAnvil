using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that calculates the n-tile.
/// </summary>
public sealed class SqlNTileWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlNTileWindowFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.NTile, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlNTileWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNTileWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlNTileWindowFunctionExpressionNode( Arguments, traits );
    }
}
