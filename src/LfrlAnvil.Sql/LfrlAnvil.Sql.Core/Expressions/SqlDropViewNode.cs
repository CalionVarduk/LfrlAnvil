namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a removal of a single view.
/// </summary>
public sealed class SqlDropViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropViewNode(SqlRecordSetInfo view, bool ifExists)
        : base( SqlNodeType.DropView )
    {
        View = view;
        IfExists = ifExists;
    }

    /// <summary>
    /// View's name.
    /// </summary>
    public SqlRecordSetInfo View { get; }

    /// <summary>
    /// Specifies whether or not the removal attempt should only be made if this view exists in DB.
    /// </summary>
    public bool IfExists { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
