using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlView : SqlView
{
    internal MySqlView(MySqlSchema schema, MySqlViewBuilder builder)
        : base( schema, builder, new MySqlViewDataFieldCollection( builder.Source ) ) { }

    public new MySqlViewDataFieldCollection DataFields => ReinterpretCast.To<MySqlViewDataFieldCollection>( base.DataFields );
    public new MySqlSchema Schema => ReinterpretCast.To<MySqlSchema>( base.Schema );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
