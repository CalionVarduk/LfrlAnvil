namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRenameTableNode : SqlNodeBase
{
    internal SqlRenameTableNode(string oldSchemaName, string oldName, string newSchemaName, string newName, bool isTemporary)
        : base( SqlNodeType.RenameTable )
    {
        OldSchemaName = oldSchemaName;
        OldName = oldName;
        NewSchemaName = newSchemaName;
        NewName = newName;
        IsTemporary = isTemporary;
    }

    // TODO: add SqlRecordSetName struct with fields: SchemaName, Name, IsTemporary + with GetIdentifier() method & static creation methods
    // e.g. temp tables don't have schema name
    // in the future, this struct can support an optional DatabaseName for cross-DB queries
    public string OldSchemaName { get; }
    public string OldName { get; }
    public string NewSchemaName { get; }
    public string NewName { get; }
    public bool IsTemporary { get; }
}
