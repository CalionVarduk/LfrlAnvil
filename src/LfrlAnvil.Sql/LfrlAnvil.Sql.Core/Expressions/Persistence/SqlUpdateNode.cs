using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree statement node that defines an update of existing records in a data source.
/// </summary>
public sealed class SqlUpdateNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlUpdateNode(SqlDataSourceNode dataSource, SqlValueAssignmentNode[] assignments)
        : base( SqlNodeType.Update )
    {
        DataSource = dataSource;
        Assignments = assignments;
    }

    /// <summary>
    /// Data source that defines records to be updated.
    /// </summary>
    /// <remarks>Records will be updated in the <see cref="SqlDataSourceNode.From"/> record set.</remarks>
    public SqlDataSourceNode DataSource { get; }

    /// <summary>
    /// Collection of value assignments that this update refers to.
    /// </summary>
    public ReadOnlyArray<SqlValueAssignmentNode> Assignments { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
