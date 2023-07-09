using System.Text;

namespace LfrlAnvil.Sql.Expressions.Arithmetic;

public sealed class SqlAddExpressionNode : SqlExpressionNode
{
    internal SqlAddExpressionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.Add )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "+", Right, indent );
    }
}
