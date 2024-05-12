using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of an aggregate function that returns an average value.
/// </summary>
public sealed class SqlAverageAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlAverageAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Average, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlAverageAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlAverageAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlAverageAggregateFunctionExpressionNode( Arguments, traits );
    }
}
