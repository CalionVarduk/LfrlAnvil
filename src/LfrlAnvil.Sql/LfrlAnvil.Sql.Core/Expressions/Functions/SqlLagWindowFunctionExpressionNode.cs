using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that calculates the lag value.
/// </summary>
public sealed class SqlLagWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlLagWindowFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Lag, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlLagWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlLagWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlLagWindowFunctionExpressionNode( Arguments, traits );
    }
}
