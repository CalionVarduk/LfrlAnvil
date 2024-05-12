namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a unary bitwise not operation.
/// </summary>
public sealed class SqlBitwiseNotExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseNotExpressionNode(SqlExpressionNode value)
        : base( SqlNodeType.BitwiseNot )
    {
        Value = value;
    }

    /// <summary>
    /// Operand.
    /// </summary>
    public SqlExpressionNode Value { get; }
}
