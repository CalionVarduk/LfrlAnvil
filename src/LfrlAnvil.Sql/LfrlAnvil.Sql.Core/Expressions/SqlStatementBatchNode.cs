namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a collection of SQL statements.
/// </summary>
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

    /// <summary>
    /// Collection of SQL statements.
    /// </summary>
    public ReadOnlyArray<ISqlStatementNode> Statements { get; }

    /// <inheritdoc />
    public int QueryCount { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
}
