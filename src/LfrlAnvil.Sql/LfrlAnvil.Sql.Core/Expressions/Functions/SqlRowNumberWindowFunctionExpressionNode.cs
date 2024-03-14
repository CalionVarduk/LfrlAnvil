using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlRowNumberWindowFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlRowNumberWindowFunctionExpressionNode(Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.RowNumber, ReadOnlyArray<SqlExpressionNode>.Empty, traits ) { }

    [Pure]
    public override SqlRowNumberWindowFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlRowNumberWindowFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlRowNumberWindowFunctionExpressionNode( traits );
    }
}
