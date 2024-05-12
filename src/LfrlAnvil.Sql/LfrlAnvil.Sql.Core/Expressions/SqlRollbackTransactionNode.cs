namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a rollback of a DB transaction.
/// </summary>
public sealed class SqlRollbackTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRollbackTransactionNode()
        : base( SqlNodeType.RollbackTransaction ) { }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
