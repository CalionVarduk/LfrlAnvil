using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlCheck : MySqlConstraint, ISqlCheck
{
    internal MySqlCheck(MySqlTable table, MySqlCheckBuilder builder)
        : base( table, builder ) { }

    public override MySqlDatabase Database => Table.Database;
}
