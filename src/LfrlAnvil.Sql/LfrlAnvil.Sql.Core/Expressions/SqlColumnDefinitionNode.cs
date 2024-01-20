namespace LfrlAnvil.Sql.Expressions;

public class SqlColumnDefinitionNode : SqlNodeBase
{
    protected internal SqlColumnDefinitionNode(string name, TypeNullability type, SqlExpressionNode? defaultValue)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = type;
        TypeDefinition = null;
        DefaultValue = defaultValue;
    }

    protected internal SqlColumnDefinitionNode(
        string name,
        ISqlColumnTypeDefinition typeDefinition,
        bool isNullable,
        SqlExpressionNode? defaultValue)
        : base( SqlNodeType.ColumnDefinition )
    {
        Name = name;
        Type = TypeNullability.Create( typeDefinition.RuntimeType, isNullable );
        TypeDefinition = typeDefinition;
        DefaultValue = defaultValue;
    }

    public string Name { get; }
    public TypeNullability Type { get; }
    public ISqlColumnTypeDefinition? TypeDefinition { get; }
    public SqlExpressionNode? DefaultValue { get; }
}
