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
}
