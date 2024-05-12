namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical check if <see cref="Value"/>
/// satisfies a string <see cref="Pattern"/>.
/// </summary>
public sealed class SqlLikeConditionNode : SqlConditionNode
{
    internal SqlLikeConditionNode(SqlExpressionNode value, SqlExpressionNode pattern, SqlExpressionNode? escape, bool isNegated)
        : base( SqlNodeType.Like )
    {
        Value = value;
        Pattern = pattern;
        Escape = escape;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// String pattern to check the <see cref="Value"/> against.
    /// </summary>
    public SqlExpressionNode Pattern { get; }

    /// <summary>
    /// Optional escape character for the <see cref="Pattern"/>.
    /// </summary>
    public SqlExpressionNode? Escape { get; }

    /// <summary>
    /// Specifies whether or not this node represents a check if <see cref="Value"/> does not satisfy a string <see cref="Pattern"/>.
    /// </summary>
    public bool IsNegated { get; }
}
