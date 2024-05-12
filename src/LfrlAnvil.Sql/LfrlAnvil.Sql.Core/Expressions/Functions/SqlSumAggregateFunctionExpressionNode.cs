using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of an aggregate function that returns the sum value.
/// </summary>
public sealed class SqlSumAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlSumAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Sum, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlSumAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlSumAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlSumAggregateFunctionExpressionNode( Arguments, traits );
    }
}
