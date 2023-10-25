namespace LfrlAnvil.Sql.Expressions;

public sealed class SqlDropViewNode : SqlNodeBase
{
    internal SqlDropViewNode(SqlRecordSetInfo view, bool ifExists)
        : base( SqlNodeType.DropView )
    {
        View = view;
        IfExists = ifExists;
    }

    public SqlRecordSetInfo View { get; }
    public bool IfExists { get; }
}
