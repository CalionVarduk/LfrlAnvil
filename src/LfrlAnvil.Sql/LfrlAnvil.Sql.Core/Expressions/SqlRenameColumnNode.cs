namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRenameColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRenameColumnNode(SqlRecordSetInfo table, string oldName, string newName)
        : base( SqlNodeType.RenameColumn )
    {
        Table = table;
        OldName = oldName;
        NewName = newName;
    }

    public SqlRecordSetInfo Table { get; }
    public string OldName { get; }
    public string NewName { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
