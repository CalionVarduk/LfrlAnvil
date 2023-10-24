namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropColumnNode : SqlNodeBase
{
    internal SqlDropColumnNode(string schemaName, string tableName, string name, bool isTableTemporary)
        : base( SqlNodeType.DropColumn )
    {
        SchemaName = schemaName;
        TableName = tableName;
        Name = name;
        IsTableTemporary = isTableTemporary;
    }

    public string SchemaName { get; }
    public string TableName { get; }
    public string Name { get; }
    public bool IsTableTemporary { get; }
}
