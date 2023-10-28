namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropColumnNode(SqlRecordSetInfo table, string name)
        : base( SqlNodeType.DropColumn )
    {
        Table = table;
        Name = name;
    }

    public SqlRecordSetInfo Table { get; }
    public string Name { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
