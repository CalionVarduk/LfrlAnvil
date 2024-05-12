namespace LfrlAnvil.Sql.Expressions.Arithmetic;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a unary arithmetic negate operation.
/// </summary>
public sealed class SqlNegateExpressionNode : SqlExpressionNode
{
    internal SqlNegateExpressionNode(SqlExpressionNode value)
        : base( SqlNodeType.Negate )
    {
        Value = value;
    }

    /// <summary>
    /// Operand.
    /// </summary>
    public SqlExpressionNode Value { get; }
}
