using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a conversion of <see cref="SqlConditionNode"/>
/// to <see cref="SqlExpressionNode"/>.
/// </summary>
public sealed class SqlConditionValueNode : SqlExpressionNode
{
    internal SqlConditionValueNode(SqlConditionNode condition)
        : base( SqlNodeType.ConditionValue )
    {
        Condition = condition;
    }

    /// <summary>
    /// Underlying condition.
    /// </summary>
    public SqlConditionNode Condition { get; }
}
