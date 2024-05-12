namespace LfrlAnvil.Sql.Expressions.Logical;

/// <summary>
/// Represents an SQL syntax tree condition node that defines a logical check if <see cref="Value"/> exists
/// in a set of values returned by a sub-query.
/// </summary>
public sealed class SqlInQueryConditionNode : SqlConditionNode
{
    internal SqlInQueryConditionNode(SqlExpressionNode value, SqlQueryExpressionNode query, bool isNegated)
        : base( SqlNodeType.InQuery )
    {
        Value = value;
        Query = query;
        IsNegated = isNegated;
    }

    /// <summary>
    /// Value to check.
    /// </summary>
    public SqlExpressionNode Value { get; }

    /// <summary>
    /// Sub-query that the <see cref="Value"/> is compared against.
    /// </summary>
    public SqlQueryExpressionNode Query { get; }

    /// <summary>
    /// Specifies whether or not this node represents a check if <see cref="Value"/> does not exist
    /// in set of values returned by a sub-query.
    /// </summary>
    public bool IsNegated { get; }
}
