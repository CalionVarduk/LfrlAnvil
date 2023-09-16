namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlBetweenConditionNode : SqlConditionNode
{
    internal SqlBetweenConditionNode(SqlExpressionNode value, SqlExpressionNode min, SqlExpressionNode max, bool isNegated)
        : base( SqlNodeType.Between )
    {
        Value = value;
        Min = min;
        Max = max;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public SqlExpressionNode Min { get; }
    public SqlExpressionNode Max { get; }
    public bool IsNegated { get; }
}
