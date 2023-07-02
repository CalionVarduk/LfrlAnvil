using System.Text;

namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseLeftShiftExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseLeftShiftExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseLeftShift )
    {
        Type = left.Type is null || right.Type is null
            ? null
            : SqlExpressionType.Create( left.Type.Value.BaseType, left.Type.Value.IsNullable || right.Type.Value.IsNullable );

        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }
    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "<<", Right, indent );
    }
}
