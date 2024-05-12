using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that returns the rank of the row.
/// </summary>
public sealed class SqlRankWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlRankWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Rank, ReadOnlyArray<SqlExpressionNode>.Empty, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlRankWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRankWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlRankWindowFunctionExpressionNode( traits );
    }
}
