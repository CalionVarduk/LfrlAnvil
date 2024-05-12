namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines renaming of a single table column.
/// </summary>
public sealed class SqlRenameColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRenameColumnNode(SqlRecordSetInfo table, string oldName, string newName)
        : base( SqlNodeType.RenameColumn )
    {
        Table = table;
        OldName = oldName;
        NewName = newName;
    }

    /// <summary>
    /// Source table.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Column's old name.
    /// </summary>
    public string OldName { get; }

    /// <summary>
    /// Column's new name.
    /// </summary>
    public string NewName { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
