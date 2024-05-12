using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that returns the row number.
/// </summary>
public sealed class SqlRowNumberWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlRowNumberWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.RowNumber, ReadOnlyArray<SqlExpressionNode>.Empty, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlRowNumberWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlRowNumberWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlRowNumberWindowFunctionExpressionNode( traits );
    }
}
