using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawStatementNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRawStatementNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawStatement )
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
