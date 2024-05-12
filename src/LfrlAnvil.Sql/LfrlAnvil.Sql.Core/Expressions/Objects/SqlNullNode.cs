namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a null value.
/// </summary>
public sealed class SqlNullNode : SqlExpressionNode
{
    internal SqlNullNode()
        : base( SqlNodeType.Null ) { }
}
