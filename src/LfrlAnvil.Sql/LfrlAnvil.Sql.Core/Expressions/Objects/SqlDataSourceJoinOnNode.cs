using LfrlAnvil.Sql.Expressions.Logical;

namespace LfrlAnvil.Sql.Expressions.Objects;

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

    public SqlJoinType JoinType { get; }
    public SqlRecordSetNode InnerRecordSet { get; }
    public SqlConditionNode OnExpression { get; }
}
