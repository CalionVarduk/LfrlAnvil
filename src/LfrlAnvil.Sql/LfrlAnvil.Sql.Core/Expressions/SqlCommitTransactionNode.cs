namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCommitTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCommitTransactionNode()
        : base( SqlNodeType.CommitTransaction ) { }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
