using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree expression node that defines a raw SQL query expression.
/// </summary>
public sealed class SqlRawQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlRawQueryExpressionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawQuery )
    {
        Sql = sql;
        Parameters = parameters;
    }

    /// <summary>
    /// Raw SQL query expression.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Collection of parameter nodes.
    /// </summary>
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }

    /// <inheritdoc />
    public override ReadOnlyArray<SqlSelectNode> Selection => ReadOnlyArray<SqlSelectNode>.Empty;
}
