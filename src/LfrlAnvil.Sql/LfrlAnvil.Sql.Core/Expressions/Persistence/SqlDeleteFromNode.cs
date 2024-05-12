using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a deletion of records from a data source.
/// </summary>
public sealed class SqlDeleteFromNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDeleteFromNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.DeleteFrom )
    {
        DataSource = dataSource;
    }

    /// <summary>
    /// Data source that defines records to be deleted.
    /// </summary>
    /// <remarks>Records will be removed from the <see cref="SqlDataSourceNode.From"/> record set.</remarks>
    public SqlDataSourceNode DataSource { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
