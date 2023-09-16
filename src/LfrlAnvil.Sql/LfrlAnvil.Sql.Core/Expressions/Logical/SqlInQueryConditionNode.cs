namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlInQueryConditionNode : SqlConditionNode
{
    internal SqlInQueryConditionNode(SqlExpressionNode value, SqlQueryExpressionNode query, bool isNegated)
        : base( SqlNodeType.InQuery )
    {
        Value = value;
        Query = query;
        IsNegated = isNegated;
    }

    public SqlExpressionNode Value { get; }
    public SqlQueryExpressionNode Query { get; }
    public bool IsNegated { get; }
}
