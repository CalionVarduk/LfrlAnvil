namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a commit of a DB transaction.
/// </summary>
public sealed class SqlCommitTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCommitTransactionNode()
        : base( SqlNodeType.CommitTransaction ) { }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
