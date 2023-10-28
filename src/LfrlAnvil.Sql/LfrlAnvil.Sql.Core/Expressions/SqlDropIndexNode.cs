namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropIndexNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropIndexNode(SqlSchemaObjectName name, bool ifExists)
        : base( SqlNodeType.DropIndex )
    {
        Name = name;
        IfExists = ifExists;
    }

    public SqlSchemaObjectName Name { get; }
    public bool IfExists { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
