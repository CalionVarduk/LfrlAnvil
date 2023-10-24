namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlAddColumnNode : SqlNodeBase
{
    internal SqlAddColumnNode(string schemaName, string tableName, SqlColumnDefinitionNode definition, bool isTableTemporary)
        : base( SqlNodeType.AddColumn )
    {
        SchemaName = schemaName;
        TableName = tableName;
        Definition = definition;
        IsTableTemporary = isTableTemporary;
    }

    public string SchemaName { get; }
    public string TableName { get; }
    public SqlColumnDefinitionNode Definition { get; }
    public bool IsTableTemporary { get; }
}
