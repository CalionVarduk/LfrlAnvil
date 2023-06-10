using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlGreaterThanConditionNode : SqlConditionNode
{
    internal SqlGreaterThanConditionNode(SqlExpressionNode left, SqlExpressionNode right)
        : base( SqlNodeType.GreaterThan )
    {
        Left = left;
        Right = right;
    }

    public SqlExpressionNode Left { get; }
    public SqlExpressionNode Right { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, ">", Right, indent );
    }
}
