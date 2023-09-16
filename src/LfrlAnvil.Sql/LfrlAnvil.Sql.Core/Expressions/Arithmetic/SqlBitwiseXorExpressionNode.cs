namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseXorExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseXorExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseXor )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
