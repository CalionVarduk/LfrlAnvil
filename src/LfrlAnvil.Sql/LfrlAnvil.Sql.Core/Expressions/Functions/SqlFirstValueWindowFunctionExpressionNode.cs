using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that calculates the first value.
/// </summary>
public sealed class SqlFirstValueWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlFirstValueWindowFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.FirstValue, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlFirstValueWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlFirstValueWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlFirstValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
