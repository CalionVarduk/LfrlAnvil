namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlParameterNode : SqlExpressionNode
{
    internal SqlParameterNode(string name, TypeNullability? type, int? index)
        : base( SqlNodeType.Parameter )
    {
        if ( index is not null )
            Ensure.IsGreaterThanOrEqualTo( index.Value, 0 );

        Name = name;
        Type = type;
        Index = index;
    }

    public string Name { get; }
    public TypeNullability? Type { get; }
    public int? Index { get; }
}
