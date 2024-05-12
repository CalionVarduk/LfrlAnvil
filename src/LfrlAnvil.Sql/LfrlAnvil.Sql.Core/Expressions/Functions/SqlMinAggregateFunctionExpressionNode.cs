using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of an aggregate function that returns the minimum value.
/// </summary>
public sealed class SqlMinAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlMinAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Min, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlMinAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlMinAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlMinAggregateFunctionExpressionNode( Arguments, traits );
    }
}
