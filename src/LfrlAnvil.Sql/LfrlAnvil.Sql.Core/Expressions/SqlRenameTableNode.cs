namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlRenameTableNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlRenameTableNode(SqlRecordSetInfo table, SqlSchemaObjectName newName)
        : base( SqlNodeType.RenameTable )
    {
        Table = table;
        NewName = newName;
    }

    public SqlRecordSetInfo Table { get; }
    public SqlSchemaObjectName NewName { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
