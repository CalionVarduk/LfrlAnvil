namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines a removal of a single index.
/// </summary>
public sealed class SqlDropIndexNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropIndexNode(SqlRecordSetInfo table, SqlSchemaObjectName name, bool ifExists)
        : base( SqlNodeType.DropIndex )
    {
        Table = table;
        Name = name;
        IfExists = ifExists;
    }

    /// <summary>
    /// Source table.
    /// </summary>
    public SqlRecordSetInfo Table { get; }

    /// <summary>
    /// Index's name.
    /// </summary>
    public SqlSchemaObjectName Name { get; }

    /// <summary>
    /// Specifies whether or not the removal attempt should only be made if this index exists in DB.
    /// </summary>
    public bool IfExists { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
