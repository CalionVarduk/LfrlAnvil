namespace LfrlAnvil.Sql.Expressions.Logical;

public sealed class SqlExistsConditionNode : SqlConditionNode
{
    internal SqlExistsConditionNode(SqlQueryExpressionNode query, bool isNegated)
        : base( SqlNodeType.Exists )
    {
        Query = query;
        IsNegated = isNegated;
    }

    public SqlQueryExpressionNode Query { get; }
    public bool IsNegated { get; }
}
