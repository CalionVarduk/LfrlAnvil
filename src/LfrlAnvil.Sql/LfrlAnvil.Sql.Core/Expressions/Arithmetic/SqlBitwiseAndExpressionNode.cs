using System.Text;

namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlBitwiseAndExpressionNode : SqlExpressionNode
{
    internal SqlBitwiseAndExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.BitwiseAnd )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "&", Right, indent );
    }
}
