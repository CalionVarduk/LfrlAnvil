namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary bitwise and operation.
/// </summary>
public sealed class SqlBitwiseAndExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseAndExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseAnd )
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
