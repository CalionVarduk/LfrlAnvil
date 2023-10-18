﻿using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlMinAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlMinAggregateFunctionExpressionNode(ReadOnlyMemory<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Min, arguments, traits ) { }

    [Pure]
    public override SqlMinAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlMinAggregateFunctionExpressionNode( Arguments, traits );
    }
}