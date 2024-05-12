namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a removal of a single table.
/// </summary>
public sealed class SqlDropTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropTableNode(SqlRecordSetInfo table, bool ifExists)
        : base( SqlNodeType.DropTable )
    {
        Table = table;
        IfExists = ifExists;
    }

    /// <summary>
    /// Table's name.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Specifies whether or not the removal attempt should only be made if this table exists in DB.
    /// </summary>
    public bool IfExists { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
