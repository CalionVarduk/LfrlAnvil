namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single common table expression (CTE).
/// </summary>
public abstract class SqlCommonTableExpressionNode : SqlNodeBase
{
    internal SqlCommonTableExpressionNode(SqlQueryExpressionNode query, string name, bool isRecursive)
        : base( SqlNodeType.CommonTableExpression )
    {
        Query = query;
        Name = name;
        IsRecursive = isRecursive;
        RecordSet = new SqlCommonTableExpressionRecordSetNode( this, alias: null, isOptional: false );
    }

    /// <summary>
    /// Underlying query that defines this common table expression.
    /// </summary>
    public SqlQueryExpressionNode Query { get; }

    /// <summary>
    /// Name of this common table expression.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specifies whether or not this common table expression is recursive.
    /// </summary>
    public bool IsRecursive { get; }

    /// <summary>
    /// <see cref="SqlCommonTableExpressionRecordSetNode"/> instance associated with this common table expression.
    /// </summary>
    public SqlCommonTableExpressionRecordSetNode RecordSet { get; }
}
