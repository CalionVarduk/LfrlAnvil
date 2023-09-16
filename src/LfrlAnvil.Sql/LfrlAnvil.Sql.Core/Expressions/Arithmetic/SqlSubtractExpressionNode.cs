namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlSubtractExpressionNode : SqlExpressionNode
{
    internal SqlSubtractExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Subtract )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
