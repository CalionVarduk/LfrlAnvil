using System.Text;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlConditionValueNode : SqlExpressionNode
{
    internal SqlConditionValueNode(SqlConditionNode condition)
        : base( SqlNodeType.ConditionValue )
    {
        Condition = condition;
        Type = SqlExpressionType.Create<bool>();
    }

    public SqlConditionNode Condition { get; }
    public override SqlExpressionType? Type { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "VALUE" ), Condition, indent );
    }
}
