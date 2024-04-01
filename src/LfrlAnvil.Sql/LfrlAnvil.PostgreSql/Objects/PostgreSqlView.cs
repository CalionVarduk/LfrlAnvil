using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlView : SqlView
{
    internal PostgreSqlView(PostgreSqlSchema schema, PostgreSqlViewBuilder builder)
        : base( schema, builder, new PostgreSqlViewDataFieldCollection( builder.Source ) ) { }

    public new PostgreSqlViewDataFieldCollection DataFields => ReinterpretCast.To<PostgreSqlViewDataFieldCollection>( base.DataFields );
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
