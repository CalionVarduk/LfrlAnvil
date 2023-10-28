namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRollbackTransactionNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRollbackTransactionNode()
        : base( SqlNodeType.RollbackTransaction ) { }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
