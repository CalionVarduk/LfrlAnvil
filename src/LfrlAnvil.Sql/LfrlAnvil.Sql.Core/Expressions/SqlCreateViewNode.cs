namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateViewNode(SqlRecordSetInfo info, bool ifNotExists, SqlQueryExpressionNode source)
        : base( SqlNodeType.CreateView )
    {
        Info = info;
        IfNotExists = ifNotExists;
        Source = source;
    }

    public SqlRecordSetInfo Info { get; }
    public bool IfNotExists { get; }
    public SqlQueryExpressionNode Source { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
