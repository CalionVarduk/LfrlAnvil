namespace LfrlAnvil.Sql.Expressions;

public class SqlColumnDefinitionNode : SqlNodeBase
{
    protected internal SqlColumnDefinitionNode(string name, SqlExpressionType type, SqlExpressionNode? defaultValue)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public SqlExpressionType Type { get; }
    public SqlExpressionNode? DefaultValue { get; }
}
