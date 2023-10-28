namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlAddColumnNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlAddColumnNode(SqlRecordSetInfo table, SqlColumnDefinitionNode definition)
        : base( SqlNodeType.AddColumn )
    {
        Table = table;
        Definition = definition;
    }

    public SqlRecordSetInfo Table { get; }
    public SqlColumnDefinitionNode Definition { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
