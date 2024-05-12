using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of an aggregate function that returns the number of records.
/// </summary>
public sealed class SqlCountAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlCountAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Count, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlCountAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlCountAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlCountAggregateFunctionExpressionNode( Arguments, traits );
    }
}
