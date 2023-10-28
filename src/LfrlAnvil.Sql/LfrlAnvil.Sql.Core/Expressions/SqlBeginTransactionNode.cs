using System.Data;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlBeginTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlBeginTransactionNode(IsolationLevel isolationLevel)
        : base( SqlNodeType.BeginTransaction )
    {
        IsolationLevel = isolationLevel;
    }

    public IsolationLevel IsolationLevel { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
