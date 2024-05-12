namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical check if any record exists in a sub-query.
/// </summary>
public sealed class SqlExistsConditionNode : SqlConditionNode
{
    internal SqlExistsConditionNode(SqlQueryExpressionNode query, bool isNegated)
        : base( SqlNodeType.Exists )
    {
        Query = query;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Sub-query to check.
    /// </summary>
    public SqlQueryExpressionNode Query { get; }

    /// <summary>
    /// Specifies whether or not this node represents a check if no records exist in a sub-query.
    /// </summary>
    public bool IsNegated { get; }
}
