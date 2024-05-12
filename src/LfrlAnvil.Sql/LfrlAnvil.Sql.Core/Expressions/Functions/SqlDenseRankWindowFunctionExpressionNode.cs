using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that returns the dense rank of the row.
/// </summary>
public sealed class SqlDenseRankWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlDenseRankWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.DenseRank, ReadOnlyArray<SqlExpressionNode>.Empty, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlDenseRankWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlDenseRankWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlDenseRankWindowFunctionExpressionNode( traits );
    }
}
