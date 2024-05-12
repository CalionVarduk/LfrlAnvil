namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a binary logical equal to comparison.
/// </summary>
public sealed class SqlEqualToConditionNode : SqlConditionNode
{
    internal SqlEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.EqualTo )
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
