namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlNegateExpressionNode : SqlExpressionNode
{
    internal SqlNegateExpressionNode(SqlExpressionNode value)
        : base( SqlNodeType.Negate )
    {
        Value = value;
    }

    public SqlExpressionNode Value { get; }
}
