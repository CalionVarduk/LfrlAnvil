using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlSchema : SqlSchema
{
    internal MySqlSchema(MySqlDatabase database, MySqlSchemaBuilder builder)
        : base( database, builder, new MySqlObjectCollection( builder.Objects ) ) { }

    public new MySqlObjectCollection Objects => ReinterpretCast.To<MySqlObjectCollection>( base.Objects );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
