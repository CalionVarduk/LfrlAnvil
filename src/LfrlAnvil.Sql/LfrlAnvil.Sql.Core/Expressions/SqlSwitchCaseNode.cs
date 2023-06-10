using System.Text;
using LfrlAnvil.Extensions;
using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlSwitchCaseNode : SqlNodeBase
{
    internal SqlSwitchCaseNode(SqlConditionNode condition, SqlExpressionNode expression)
        : base( SqlNodeType.SwitchCase )
    {
        Condition = condition;
        Expression = expression;
    }

    public SqlConditionNode Condition { get; }
    public SqlExpressionNode Expression { get; }

    protected override void ToString(StringBuilder builder, int indent)
    {
        var thenIndent = indent + DefaultIndent;
        AppendChildTo( builder.Append( "WHEN" ).Append( ' ' ), Condition, indent );
        AppendChildTo( builder.Indent( thenIndent ).Append( "THEN" ).Append( ' ' ), Expression, thenIndent );
    }
}
