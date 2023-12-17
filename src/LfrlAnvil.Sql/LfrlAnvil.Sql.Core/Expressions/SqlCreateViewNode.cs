namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlCreateViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateViewNode(SqlRecordSetInfo info, bool replaceIfExists, SqlQueryExpressionNode source)
        : base( SqlNodeType.CreateView )
    {
        Info = info;
        ReplaceIfExists = replaceIfExists;
        Source = source;
    }

    public SqlRecordSetInfo Info { get; }
    public bool ReplaceIfExists { get; }
    public SqlQueryExpressionNode Source { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
