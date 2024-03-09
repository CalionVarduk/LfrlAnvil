using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlViewDataField : SqlViewDataField
{
    internal MySqlViewDataField(MySqlView view, string name)
        : base( view, name ) { }

    public new MySqlView View => ReinterpretCast.To<MySqlView>( base.View );
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
