using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlColumn : SqlColumn
{
    internal PostgreSqlColumn(PostgreSqlTable table, PostgreSqlColumnBuilder builder)
        : base( table, builder ) { }

    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
