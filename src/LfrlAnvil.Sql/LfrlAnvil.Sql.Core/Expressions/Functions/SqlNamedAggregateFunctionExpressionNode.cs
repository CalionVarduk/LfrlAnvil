using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines an invocation of a custom aggregate function.
/// </summary>
public sealed class SqlNamedAggregateFunctionExpressionNode : SqlAggregateFunctionExpressionNode
{
    internal SqlNamedAggregateFunctionExpressionNode(
        SqlSchemaObjectName name,
        ReadOnlyArray<SqlExpressionNode> arguments,
        Chain<SqlTraitNode> traits)
        : base( SqlFunctionType.Named, arguments, traits )
    {
        Name = name;
    }

    /// <summary>
    /// Aggregate function's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    /// <inheritdoc />
    [Pure]
    public override SqlNamedAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlNamedAggregateFunctionExpressionNode( Name, Arguments, traits );
    }
}
