using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Objects.Builders;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlCheck : MySqlObject, ISqlCheck
{
    internal MySqlCheck(MySqlTable table, MySqlCheckBuilder builder)
        : base( builder )
    {
        Table = table;
        FullName = builder.FullName;
    }

    public MySqlTable Table { get; }
    public override string FullName { get; }
    public override MySqlDatabase Database => Table.Database;

    ISqlTable ISqlCheck.Table => Table;
}
