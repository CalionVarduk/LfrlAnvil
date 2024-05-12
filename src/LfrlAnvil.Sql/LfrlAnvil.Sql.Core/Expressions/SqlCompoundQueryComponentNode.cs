namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree node that defines a query that belongs to a compound query.
/// </summary>
public sealed class SqlCompoundQueryComponentNode : SqlNodeBase
{
    internal SqlCompoundQueryComponentNode(SqlQueryExpressionNode query, SqlCompoundQueryOperator @operator)
        : base( SqlNodeType.CompoundQueryComponent )
    {
        Query = query;
        Operator = @operator;
    }

    /// <summary>
    /// Underlying query.
    /// </summary>
    public SqlQueryExpressionNode Query { get; }

    /// <summary>
    /// Compound query operator with which this <see cref="Query"/> should be included.
    /// </summary>
    public SqlCompoundQueryOperator Operator { get; }
}
