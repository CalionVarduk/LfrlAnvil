using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a window function that calculates the n-th value.
/// </summary>
public sealed class SqlNthValueWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlNthValueWindowFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.NthValue, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlNthValueWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNthValueWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlNthValueWindowFunctionExpressionNode( Arguments, traits );
    }
}
