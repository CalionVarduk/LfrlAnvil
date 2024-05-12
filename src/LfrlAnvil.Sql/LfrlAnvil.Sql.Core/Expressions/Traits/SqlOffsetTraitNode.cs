namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single offset trait.
/// </summary>
public sealed class SqlOffsetTraitNode : SqlTraitNode
{
    internal SqlOffsetTraitNode(SqlExpressionNode value)
        : base( SqlNodeType.OffsetTrait )
    {
        Value = value;
    }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public SqlExpressionNode Value { get; }
}
