namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlParameterNode : SqlExpressionNode
{
    internal SqlParameterNode(string name, SqlExpressionType? type)
        : base( SqlNodeType.Parameter )
    {
        Name = name;
        Type = type;
    }

    public string Name { get; }
    public SqlExpressionType? Type { get; }
}
