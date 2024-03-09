using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlTable : SqlTable
{
    internal MySqlTable(MySqlSchema schema, MySqlTableBuilder builder)
        : base( schema, builder, new MySqlColumnCollection( builder.Columns ), new MySqlConstraintCollection( builder.Constraints ) ) { }

    public new MySqlColumnCollection Columns => ReinterpretCast.To<MySqlColumnCollection>( base.Columns );
    public new MySqlConstraintCollection Constraints => ReinterpretCast.To<MySqlConstraintCollection>( base.Constraints );
    public new MySqlSchema Schema => ReinterpretCast.To<MySqlSchema>( base.Schema );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
