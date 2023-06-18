using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Decorators;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlExtendableQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlExtendableQueryExpressionNode(SqlNodeType nodeType, Chain<SqlQueryDecoratorNode> decorators)
        : base( nodeType )
    {
        Decorators = decorators;
    }

    public Chain<SqlQueryDecoratorNode> Decorators { get; }

    [Pure]
    public abstract SqlExtendableQueryExpressionNode Decorate(SqlQueryDecoratorNode decorator);
}
