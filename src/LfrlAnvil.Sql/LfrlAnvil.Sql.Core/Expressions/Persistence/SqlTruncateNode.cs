using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a truncation of a table.
/// </summary>
public sealed class SqlTruncateNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlTruncateNode(SqlRecordSetNode table)
        : base( SqlNodeType.Truncate )
    {
        Table = table;
    }

    /// <summary>
    /// Table to truncate.
    /// </summary>
    public SqlRecordSetNode Table { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
