﻿using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public abstract class SqlAggregateFunctionExpressionNode : SqlExpressionNode
{
    protected SqlAggregateFunctionExpressionNode(
        SqlFunctionType functionType,
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlTraitNode> traits)
        : base( SqlNodeType.AggregateFunctionExpression )
    {
        FunctionType = functionType;
        Arguments = arguments;
        Traits = traits;
    }

    public SqlFunctionType FunctionType { get; }
    public ReadOnlyMemory<SqlExpressionNode> Arguments { get; }
    public Chain<SqlTraitNode> Traits { get; }

    [Pure]
    public abstract SqlAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait);
}