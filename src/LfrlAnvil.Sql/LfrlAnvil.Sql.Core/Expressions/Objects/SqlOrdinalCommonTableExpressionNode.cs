namespace LfrlAnvil.Sql.Expressions.Objects;

/// <summary>
/// Represents an SQL syntax tree node that defines a single ordinal (non-recursive) common table expression (CTE).
/// </summary>
public sealed class SqlOrdinalCommonTableExpressionNode : SqlCommonTableExpressionNode
{
    internal SqlOrdinalCommonTableExpressionNode(SqlQueryExpressionNode query, string name)
        : base( query, name, isRecursive: false ) { }
}
