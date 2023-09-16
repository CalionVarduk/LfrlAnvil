using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Traits;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlExtendableQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlExtendableQueryExpressionNode(SqlNodeType nodeType, Chain<SqlTraitNode> traits)
        : base( nodeType )
    {
        Traits = traits;
    }

    public Chain<SqlTraitNode> Traits { get; }

    [Pure]
    public abstract SqlExtendableQueryExpressionNode AddTrait(SqlTraitNode trait);
}
