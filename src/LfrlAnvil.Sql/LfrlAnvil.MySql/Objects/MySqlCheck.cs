using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlCheck : SqlCheck
{
    internal MySqlCheck(MySqlTable table, MySqlCheckBuilder builder)
        : base( table, builder ) { }

    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
