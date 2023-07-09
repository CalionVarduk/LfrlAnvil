using System.Text;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlConditionValueNode : SqlExpressionNode
{
    internal SqlConditionValueNode(SqlConditionNode condition)
        : base( SqlNodeType.ConditionValue )
    {
        Condition = condition;
    }

    public SqlConditionNode Condition { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        AppendChildTo( builder.Append( "VALUE" ), Condition, indent );
    }
}
