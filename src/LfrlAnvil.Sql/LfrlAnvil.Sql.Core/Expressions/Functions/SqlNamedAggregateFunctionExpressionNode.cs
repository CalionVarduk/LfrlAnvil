using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions.Functions;

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

    public SqlSchemaObjectName Name { get; }

    [Pure]
    public override SqlNamedAggregateFunctionExpressionNode AddTrait(SqlTraitNode trait)
    {
        return SetTraits( Traits.ToExtendable().Extend( trait ) );
    }

    [Pure]
    public override SqlNamedAggregateFunctionExpressionNode SetTraits(Chain<SqlTraitNode> traits)
    {
        return new SqlNamedAggregateFunctionExpressionNode( Name, Arguments, traits );
    }
}
