namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary bitwise left shift operation.
/// </summary>
public sealed class SqlBitwiseLeftShiftExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseLeftShiftExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseLeftShift )
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
