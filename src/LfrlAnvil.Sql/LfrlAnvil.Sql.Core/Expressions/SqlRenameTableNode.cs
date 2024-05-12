namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines renaming of a single table.
/// </summary>
public sealed class SqlRenameTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRenameTableNode(SqlRecordSetInfo table, SqlSchemaObjectName newName)
        : base( SqlNodeType.RenameTable )
    {
        Table = table;
        NewName = newName;
    }

    /// <summary>
    /// Table's old name.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Table's new name.
    /// </summary>
    public SqlSchemaObjectName NewName { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
