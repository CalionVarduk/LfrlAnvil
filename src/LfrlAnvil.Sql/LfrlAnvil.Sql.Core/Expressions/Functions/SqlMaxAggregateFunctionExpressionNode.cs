﻿using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of an aggregate function that returns the maximum value.
/// </summary>
public sealed class SqlMaxAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlMaxAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Max, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlMaxAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlMaxAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlMaxAggregateFunctionExpressionNode( Arguments, traits );
    }
}
