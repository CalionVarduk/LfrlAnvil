using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlIndex : SqlIndex
{
    internal MySqlIndex(MySqlTable table, MySqlIndexBuilder builder)
        : base( table, builder ) { }

    public new SqlIndexColumnArray<MySqlColumn> Columns => base.Columns.UnsafeReinterpretAs<MySqlColumn>();
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
