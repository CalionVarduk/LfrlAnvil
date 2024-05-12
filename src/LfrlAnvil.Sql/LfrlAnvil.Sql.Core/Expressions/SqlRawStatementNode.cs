using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a raw SQL statement.
/// </summary>
public sealed class SqlRawStatementNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRawStatementNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawStatement )
    {
        Sql = sql;
        Parameters = parameters;
    }

    /// <summary>
    /// Raw SQL statement.
    /// </summary>
    public string Sql { get; }

    /// <summary>
    /// Collection of parameter nodes.
    /// </summary>
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
