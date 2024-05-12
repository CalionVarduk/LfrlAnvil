namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical not equal to comparison.
/// </summary>
public sealed class SqlNotEqualToConditionNode : SqlConditionNode
{
    internal SqlNotEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.NotEqualTo )
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
