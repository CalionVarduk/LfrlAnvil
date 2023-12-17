namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropIndexNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropIndexNode(SqlRecordSetInfo table, SqlSchemaObjectName name, bool ifExists)
        : base( SqlNodeType.DropIndex )
    {
        Table = table;
        Name = name;
        IfExists = ifExists;
    }

    public SqlRecordSetInfo Table { get; }
    public SqlSchemaObjectName Name { get; }
    public bool IfExists { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
