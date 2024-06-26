﻿using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Expressions.Functions;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.TestExtensions.Sql.Mocks;

public sealed class SqlAggregateFunctionNodeMock : SqlAggregateFunctionExpressionNode
{
    public SqlAggregateFunctionNodeMock(ReadOnlyArray<SqlExpressionNode>? arguments = null, Chain<SqlTraitNode>? traits = null)
        : base( arguments ?? ReadOnlyArray<SqlExpressionNode>.Empty, traits ?? Chain<SqlTraitNode>.Empty ) { }

    public override SqlAggregateFunctionNodeMock SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlAggregateFunctionNodeMock( Arguments, traits );
    }
}
