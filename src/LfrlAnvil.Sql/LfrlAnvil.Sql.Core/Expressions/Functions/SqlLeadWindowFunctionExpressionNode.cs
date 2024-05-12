using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that calculates the lead value.
/// </summary>
public sealed class SqlLeadWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlLeadWindowFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Lead, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlLeadWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlLeadWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlLeadWindowFunctionExpressionNode( Arguments, traits );
    }
}
