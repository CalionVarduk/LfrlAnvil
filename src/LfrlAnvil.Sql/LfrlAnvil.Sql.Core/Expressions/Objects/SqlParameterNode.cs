namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlParameterNode : SqlExpressionNode
{
    internal SqlParameterNode(string name, TypeNullability? type)
        : base( SqlNodeType.Parameter )
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public TypeNullability? Type { get; }
}
