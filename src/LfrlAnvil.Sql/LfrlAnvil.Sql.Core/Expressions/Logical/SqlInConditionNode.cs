namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical check if <see cref="Value"/> exists in a set of values.
/// </summary>
public sealed class SqlInConditionNode : SqlConditionNode
{
    internal SqlInConditionNode(SqlExpressionNode value, SqlExpressionNode[] expressions, bool isNegated)
        : base( SqlNodeType.In )
    {
        Assume.IsNotEmpty( expressions );
        Value = value;
        Expressions = expressions;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Collection of values that the <see cref="Value"/> is compared against.
    /// </summary>
    public ReadOnlyArray<SqlExpressionNode> Expressions { get; }

    /// <summary>
    /// Specifies whether or not this node represents a check if <see cref="Value"/> does not exist in a set of values.
    /// </summary>
    public bool IsNegated { get; }
}
