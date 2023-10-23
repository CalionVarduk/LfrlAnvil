using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlTruncateNode : SqlNodeBase
{
    internal SqlTruncateNode(SqlRecordSetNode table)
        : base( SqlNodeType.Truncate )
    {
        Table = table;
    }

    public SqlRecordSetNode Table { get; }
}
