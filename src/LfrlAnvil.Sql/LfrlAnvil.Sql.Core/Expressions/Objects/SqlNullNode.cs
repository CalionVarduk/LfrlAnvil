namespace LfrlAnvil.Sql.Expressions.Objects;

public sealed class SqlNullNode : SqlExpressionNode
{
    internal SqlNullNode()
        : base( SqlNodeType.Null ) { }
}
