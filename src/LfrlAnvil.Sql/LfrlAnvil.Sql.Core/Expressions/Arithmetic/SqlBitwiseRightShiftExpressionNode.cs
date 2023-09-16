namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseRightShiftExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseRightShiftExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseRightShift )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
}
