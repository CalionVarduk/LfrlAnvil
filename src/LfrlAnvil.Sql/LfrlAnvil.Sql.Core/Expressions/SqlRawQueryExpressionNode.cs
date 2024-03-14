using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRawQueryExpressionNode : SqlQueryExpressionNode
{
    internal SqlRawQueryExpressionNode(string sql, SqlParameterNode[] parameters)
        : base( SqlNodeType.RawQuery )
    {
        Sql = sql;
        Parameters = parameters;
    }

    public string Sql { get; }
    public ReadOnlyArray<SqlParameterNode> Parameters { get; }
    public override ReadOnlyArray<SqlSelectNode> Selection => ReadOnlyArray<SqlSelectNode>.Empty;
}
