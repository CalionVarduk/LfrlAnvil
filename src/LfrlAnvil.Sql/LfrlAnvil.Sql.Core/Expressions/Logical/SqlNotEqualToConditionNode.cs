using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlNotEqualToConditionNode : SqlConditionNode
{
    internal SqlNotEqualToConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.NotEqualTo )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "<>", Right, indent );
    }
}
