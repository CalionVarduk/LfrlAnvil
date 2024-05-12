namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines an addition of a single column to a table.
/// </summary>
public sealed class SqlAddColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlAddColumnNode(SqlRecordSetInfo table, SqlColumnDefinitionNode definition)
        : base( SqlNodeType.AddColumn )
    {
        Table = table;
        Definition = definition;
    }

    /// <summary>
    /// Source table.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Definition of the column to add.
    /// </summary>
    public SqlColumnDefinitionNode Definition { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
