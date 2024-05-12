namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a removal of a single table column.
/// </summary>
public sealed class SqlDropColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropColumnNode(SqlRecordSetInfo table, string name)
        : base( SqlNodeType.DropColumn )
    {
        Table = table;
        Name = name;
    }

    /// <summary>
    /// Source table.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Column's name.
    /// </summary>
    public string Name { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
