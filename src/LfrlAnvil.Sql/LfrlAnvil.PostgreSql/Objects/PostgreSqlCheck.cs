using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlCheck : SqlCheck
{
    internal PostgreSqlCheck(PostgreSqlTable table, PostgreSqlCheckBuilder builder)
        : base( table, builder ) { }

    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
