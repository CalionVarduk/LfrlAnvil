namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical less than comparison.
/// </summary>
public sealed class SqlLessThanConditionNode : SqlConditionNode
{
    internal SqlLessThanConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.LessThan )
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
