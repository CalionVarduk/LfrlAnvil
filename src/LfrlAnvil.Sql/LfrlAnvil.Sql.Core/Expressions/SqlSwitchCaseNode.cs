using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a single switch case.
/// </summary>
public sealed class SqlSwitchCaseNode : SqlNodeBase
{
    internal SqlSwitchCaseNode(SqlConditionNode condition, SqlExpressionNode expression)
        : base( SqlNodeType.SwitchCase )
    {
        Condition = condition;
        Expression = expression;
    }

    /// <summary>
    /// Underlying condition.
    /// </summary>
    public SqlConditionNode Condition { get; }

    /// <summary>
    /// Underlying expression.
    /// </summary>
    public SqlExpressionNode Expression { get; }
}
