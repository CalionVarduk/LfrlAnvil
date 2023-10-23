namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropIndexNode : SqlNodeBase
{
    internal SqlDropIndexNode(string schemaName, string name, bool ifExists)
        : base( SqlNodeType.DropIndex )
    {
        SchemaName = schemaName;
        Name = name;
        IfExists = ifExists;
    }

    public string SchemaName { get; }
    public string Name { get; }
    public bool IfExists { get; }
}
