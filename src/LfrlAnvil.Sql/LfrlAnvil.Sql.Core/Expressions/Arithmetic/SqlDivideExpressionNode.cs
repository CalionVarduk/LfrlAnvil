namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary arithmetic divide operation.
/// </summary>
public sealed class SqlDivideExpressionNode : SqlExpressionNode
{
    internal SqlDivideExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Divide )
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
