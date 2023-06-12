using System;

namespace LfrlAnvil.Sql.Expressions;

public abstract class SqlQueryExpressionNode : SqlExpressionNode
{
    internal SqlQueryExpressionNode(SqlNodeType nodeType)
        : base( nodeType ) { }

    public abstract ReadOnlyMemory<SqlSelectNode> Selection { get; }
}
