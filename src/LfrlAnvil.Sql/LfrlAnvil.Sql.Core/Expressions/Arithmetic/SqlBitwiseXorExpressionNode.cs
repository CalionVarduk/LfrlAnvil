namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary bitwise xor operation.
/// </summary>
public sealed class SqlBitwiseXorExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseXorExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseXor )
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
