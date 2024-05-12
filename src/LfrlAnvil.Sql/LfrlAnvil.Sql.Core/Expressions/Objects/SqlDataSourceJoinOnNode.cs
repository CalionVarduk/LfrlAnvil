using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single record set join operation.
/// </summary>
public sealed class SqlDataSourceJoinOnNode : SqlNodeBase
{
    internal SqlDataSourceJoinOnNode(
        SqlJoinType joinType,
        SqlRecordSetNode innerRecordSet,
        SqlConditionNode onExpression)
        : base( SqlNodeType.JoinOn )
    {
        JoinType = joinType;
        InnerRecordSet = innerRecordSet;
        OnExpression = onExpression;
    }

    /// <summary>
    /// Type of this join operation.
    /// </summary>
    public SqlJoinType JoinType { get; }

    /// <summary>
    /// Inner <see cref="SqlRecordSetNode"/> instance.
    /// </summary>
    public SqlRecordSetNode InnerRecordSet { get; }

    /// <summary>
    /// Condition of this join operation.
    /// </summary>
    public SqlConditionNode OnExpression { get; }
}
