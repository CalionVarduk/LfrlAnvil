namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary arithmetic add operation.
/// </summary>
public sealed class SqlAddExpressionNode : SqlExpressionNode
{
    internal SqlAddExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Add )
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
