using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of an aggregate function that returns a concatenated string.
/// </summary>
public sealed class SqlStringConcatAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlStringConcatAggregateFunctionExpressionNode(ReadOnlyArray<SqlExpressionNode> arguments, Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.StringConcat, arguments, traits ) { }

    /// <inheritdoc />
    [Pure]
    public override SqlStringConcatAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlStringConcatAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlStringConcatAggregateFunctionExpressionNode( Arguments, traits );
    }
}
