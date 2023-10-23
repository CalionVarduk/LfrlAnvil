namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateViewNode : SqlNodeBase
{
    internal SqlCreateViewNode(string schemaName, string name, bool ifNotExists, SqlQueryExpressionNode source)
        : base( SqlNodeType.CreateView )
    {
        SchemaName = schemaName;
        Name = name;
        IfNotExists = ifNotExists;
        Source = source;
    }

    public string SchemaName { get; }
    public string Name { get; }
    public bool IfNotExists { get; }
    public SqlQueryExpressionNode Source { get; }
}
