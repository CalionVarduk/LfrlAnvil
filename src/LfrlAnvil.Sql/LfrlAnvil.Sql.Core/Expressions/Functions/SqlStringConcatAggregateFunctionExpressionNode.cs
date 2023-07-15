using System;
using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

public sealed class SqlStringConcatAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlStringConcatAggregateFunctionExpressionNode(
        ReadOnlyMemory<SqlExpressionNode> arguments,
        Chain<SqlAggregateFunctionTraitNode> traits)
        : base( SqlFunctionType.StringConcat, arguments, traits ) { }

    [Pure]
    public override SqlStringConcatAggregateFunctionExpressionNode AddTrait(SqlAggregateFunctionTraitNode trait)
    {
        var traits = Traits.ToExtendable().Extend( trait );
        return new SqlStringConcatAggregateFunctionExpressionNode( Arguments, traits );
    }
}
