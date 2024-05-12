using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that calculates the last value.
/// </summary>
public sealed class SqlLastValueWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlLastValueWindowFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.LastValue, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlLastValueWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlLastValueWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlLastValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
