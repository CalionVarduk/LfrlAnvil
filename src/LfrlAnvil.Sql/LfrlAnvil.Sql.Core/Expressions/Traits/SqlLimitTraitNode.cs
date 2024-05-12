namespace LfrlAnvil.Sql.Expressions.Traits;

/// <summary>
/// Represents an SQL syntax tree node that defines a single limit trait.
/// </summary>
public sealed class SqlLimitTraitNode : SqlTraitNode
{
    internal SqlLimitTraitNode(SqlExpressionNode value)
        : base( SqlNodeType.LimitTrait )
    {
        Value = value;
    }

    /// <summary>
    /// Underlying value.
    /// </summary>
    public SqlExpressionNode Value { get; }
}
