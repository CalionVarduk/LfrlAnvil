namespace LfrlAnvil.Sql.Expressions;

public class SqlColumnDefinitionNode : SqlNodeBase
{
    protected internal SqlColumnDefinitionNode(string name, TypeNullability type, SqlExpressionNode? defaultValue)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public TypeNullability Type { get; }
    public SqlExpressionNode? DefaultValue { get; }
}
