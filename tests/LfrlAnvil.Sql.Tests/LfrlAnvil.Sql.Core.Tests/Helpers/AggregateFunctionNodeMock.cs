using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Tests.Helpers;

public sealed class AggregateFunctionNodeMock : SqlAggregateFunctionExpressionNode
{
    public AggregateFunctionNodeMock(ReadOnlyMemory<SqlExpressionNode>? arguments = null, Chain<SqlTraitNode>? traits = null)
        : base( arguments ?? ReadOnlyMemory<SqlExpressionNode>.Empty, traits ?? Chain<SqlTraitNode>.Empty ) { }

    public override AggregateFunctionNodeMock SetTraits(Chain<SqlTraitNode> traits)
    {
        return new AggregateFunctionNodeMock( Arguments, traits );
    }
}
