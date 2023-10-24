namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRenameColumnNode : SqlNodeBase
{
    internal SqlRenameColumnNode(string schemaName, string tableName, string oldName, string newName, bool isTableTemporary)
        : base( SqlNodeType.RenameColumn )
    {
        SchemaName = schemaName;
        TableName = tableName;
        OldName = oldName;
        NewName = newName;
        IsTableTemporary = isTableTemporary;
    }

    public string SchemaName { get; }
    public string TableName { get; }
    public string OldName { get; }
    public string NewName { get; }
    public bool IsTableTemporary { get; }
}
