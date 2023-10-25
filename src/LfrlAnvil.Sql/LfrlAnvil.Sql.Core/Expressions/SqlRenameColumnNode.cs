namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRenameColumnNode : SqlNodeBase
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
}
