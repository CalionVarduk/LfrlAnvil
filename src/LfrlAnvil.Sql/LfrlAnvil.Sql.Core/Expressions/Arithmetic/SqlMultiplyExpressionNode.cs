namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary arithmetic multiply operation.
/// </summary>
public sealed class SqlMultiplyExpressionNode : SqlExpressionNode
{
    internal SqlMultiplyExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Multiply )
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
