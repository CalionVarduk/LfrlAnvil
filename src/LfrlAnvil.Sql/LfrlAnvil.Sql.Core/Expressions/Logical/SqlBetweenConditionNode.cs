namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical between comparison.
/// </summary>
public sealed class SqlBetweenConditionNode : SqlConditionNode
{
    internal SqlBetweenConditionNode(SqlExpressionNode value, SqlExpressionNode min, SqlExpressionNode max, bool isNegated)
        : base( SqlNodeType.Between )
    {
        Value = value;
        Min = min;
        Max = max;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Minimum acceptable value.
    /// </summary>
    public SqlExpressionNode Min { get; }

    /// <summary>
    /// Maximum acceptable value.
    /// </summary>
    public SqlExpressionNode Max { get; }

    /// <summary>
    /// Specifies whether or not this node represents a not between comparison.
    /// </summary>
    public bool IsNegated { get; }
}
