using LfrlAnvil.Sql.Expressions.Objects;

namespace LfrlAnvil.Sql.Expressions.Persistence;

public sealed class SqlDeleteFromNode : SqlNodeBase
{
    internal SqlDeleteFromNode(SqlDataSourceNode dataSource)
        : base( SqlNodeType.DeleteFrom )
    {
        DataSource = dataSource;
    }

    public SqlDataSourceNode DataSource { get; }
}
