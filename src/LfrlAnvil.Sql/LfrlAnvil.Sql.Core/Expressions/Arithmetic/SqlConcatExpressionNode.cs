namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary string concatenation operation.
/// </summary>
public sealed class SqlConcatExpressionNode : SqlExpressionNode
{
    internal SqlConcatExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Concat )
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
