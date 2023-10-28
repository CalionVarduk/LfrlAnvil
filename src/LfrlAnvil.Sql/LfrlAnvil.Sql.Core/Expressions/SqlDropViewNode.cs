namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlDropViewNode(SqlRecordSetInfo view, bool ifExists)
        : base( SqlNodeType.DropView )
    {
        View = view;
        IfExists = ifExists;
    }

    public SqlRecordSetInfo View { get; }
    public bool IfExists { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
