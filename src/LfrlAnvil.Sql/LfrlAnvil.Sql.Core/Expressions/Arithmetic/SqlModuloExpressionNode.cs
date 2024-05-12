namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a binary arithmetic modulo operation.
/// </summary>
public sealed class SqlModuloExpressionNode : SqlExpressionNode
{
    internal SqlModuloExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Modulo )
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
