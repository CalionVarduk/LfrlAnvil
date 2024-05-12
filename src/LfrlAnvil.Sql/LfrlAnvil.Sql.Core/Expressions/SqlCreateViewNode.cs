namespace LfrlAnvil.Sql.Expressions;

/// <summary>
/// Represents an SQL syntax tree statement node that defines creation of a single view.
/// </summary>
public sealed class SqlCreateViewNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlCreateViewNode(SqlRecordSetInfo info, bool replaceIfExists, SqlQueryExpressionNode source)
        : base( SqlNodeType.CreateView )
    {
        Info = info;
        ReplaceIfExists = replaceIfExists;
        Source = source;
    }

    /// <summary>
    /// View's name.
    /// </summary>
    public SqlRecordSetInfo Info { get; }

    /// <summary>
    /// Specifies whether or not the view should be replaced if it already exists in DB.
    /// </summary>
    public bool ReplaceIfExists { get; }

    /// <summary>
    /// Underlying source query expression that defines this view.
    /// </summary>
    public SqlQueryExpressionNode Source { get; }

    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
