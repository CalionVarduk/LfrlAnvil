using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlDeleteFromNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDeleteFromNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.DeleteFrom )
    {
        DataSource = dataSource;
    }

    public SqlDataSourceNode DataSource { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
