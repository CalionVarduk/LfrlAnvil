using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlViewDataField : SqlViewDataField
{
    internal PostgreSqlViewDataField(PostgreSqlView view, string name)
        : base( view, name ) { }

    public new PostgreSqlView View => ReinterpretCast.To<PostgreSqlView>( base.View );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
