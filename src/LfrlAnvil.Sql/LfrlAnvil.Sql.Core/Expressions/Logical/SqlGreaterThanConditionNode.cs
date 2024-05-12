namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical greater than comparison.
/// </summary>
public sealed class SqlGreaterThanConditionNode : SqlConditionNode
{
    internal SqlGreaterThanConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.GreaterThan )
    {
        Left = left;
        Right = right;
    }

    /// <summary>
    /// First operand.
    /// </summary>
    public SqlExpressionNode Left { get; }

    /// <summary>
    /// Second operand.
    /// </summary>
    public SqlExpressionNode Right { get; }
}
