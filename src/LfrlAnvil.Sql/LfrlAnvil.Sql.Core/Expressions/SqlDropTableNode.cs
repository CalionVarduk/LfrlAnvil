namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropTableNode : SqlNodeBase
{
    internal SqlDropTableNode(string schemaName, string name, bool ifExists, bool isTemporary)
        : base( SqlNodeType.DropTable )
    {
        SchemaName = schemaName;
        Name = name;
        IfExists = ifExists;
        IsTemporary = isTemporary;
    }

    public string SchemaName { get; }
    public string Name { get; }
    public bool IfExists { get; }
    public bool IsTemporary { get; }
}
