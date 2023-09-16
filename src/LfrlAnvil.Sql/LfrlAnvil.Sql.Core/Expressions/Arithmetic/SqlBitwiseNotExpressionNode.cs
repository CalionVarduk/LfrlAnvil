namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseNotExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseNotExpressionNode(SqlExpressionNode value)
        : base( SqlNodeType.BitwiseNot )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }
}
