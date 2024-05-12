namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical less than or equal to comparison.
/// </summary>
public sealed class SqlLessThanOrEqualToConditionNode : SqlConditionNode
{
    internal SqlLessThanOrEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.LessThanOrEqualTo )
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
