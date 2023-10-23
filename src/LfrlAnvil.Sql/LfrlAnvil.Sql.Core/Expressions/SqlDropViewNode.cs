namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropViewNode : SqlNodeBase
{
    internal SqlDropViewNode(string schemaName, string name, bool ifExists)
        : base( SqlNodeType.DropView )
    {
        SchemaName = schemaName;
        Name = name;
        IfExists = ifExists;
    }

    public string SchemaName { get; }
    public string Name { get; }
    public bool IfExists { get; }
}
