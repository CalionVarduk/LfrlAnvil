namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlStatementBatchNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlStatementBatchNode(ISqlStatementNode[] statements)
        : base( SqlNodeType.StatementBatch )
    {
        QueryCount = 0;
        Statements = statements;
        foreach ( var statement in statements )
            QueryCount += statement.QueryCount;
    }

    public ReadOnlyArray<ISqlStatementNode> Statements { get; }
    public int QueryCount { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
}
