using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlIndex : SqlIndex
{
    internal PostgreSqlIndex(PostgreSqlTable table, PostgreSqlIndexBuilder builder)
        : base( table, builder ) { }

    public new SqlIndexedArray<PostgreSqlColumn> Columns => base.Columns.UnsafeReinterpretAs<PostgreSqlColumn>();
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
