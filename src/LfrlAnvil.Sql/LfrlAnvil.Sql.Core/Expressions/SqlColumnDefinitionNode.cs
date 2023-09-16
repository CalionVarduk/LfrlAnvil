namespace LfrlAnvil.Sql.Expressions;

public class SqlColumnDefinitionNode : SqlNodeBase
{
    protected internal SqlColumnDefinitionNode(string name, SqlExpressionType type)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public SqlExpressionType Type { get; }
}
