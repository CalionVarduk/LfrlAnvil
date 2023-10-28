using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlTruncateNode : SqlNodeBase, ISqlStatementNode
{
    internal SqlTruncateNode(SqlRecordSetNode table)
        : base( SqlNodeType.Truncate )
    {
        Table = table;
    }

    public SqlRecordSetNode Table { get; }
    SqlNodeBase ISqlStatementNode.Node => this;
    int ISqlStatementNode.QueryCount => 0;
}
