namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropTableNode(SqlRecordSetInfo table, bool ifExists)
        : base( SqlNodeType.DropTable )
    {
        Table = table;
        IfExists = ifExists;
    }

    public SqlRecordSetInfo Table { get; }
    public bool IfExists { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
