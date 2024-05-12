namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary arithmetic subtract operation.
/// </summary>
public sealed class SqlSubtractExpressionNode : SqlExpressionNode
{
    internal SqlSubtractExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Subtract )
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
