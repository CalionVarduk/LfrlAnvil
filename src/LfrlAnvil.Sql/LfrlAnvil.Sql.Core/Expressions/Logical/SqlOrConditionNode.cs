using System.Text;

namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlOrConditionNode : SqlConditionNode
{
    internal SqlOrConditionNode(SqlConditionNode left, SqlConditionNode right)
        : base( SqlNodeType.Or )
    {
        Left = left;
        Right = right;
    }

    public SqlConditionNode Left { get; }
    public SqlConditionNode Right { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendInfixBinaryTo( builder, Left, "OR", Right, indent );
    }
}
