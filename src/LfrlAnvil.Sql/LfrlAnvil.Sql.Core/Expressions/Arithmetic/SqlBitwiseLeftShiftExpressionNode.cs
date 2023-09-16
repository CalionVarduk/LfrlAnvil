namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseLeftShiftExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseLeftShiftExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseLeftShift )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
