using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

public sealed class PostgreSqlSchema : SqlSchema
{
    internal PostgreSqlSchema(PostgreSqlDatabase database, PostgreSqlSchemaBuilder builder)
        : base( database, builder, new PostgreSqlObjectCollection( builder.Objects ) ) { }

    public new PostgreSqlObjectCollection Objects => ReinterpretCast.To<PostgreSqlObjectCollection>( base.Objects );
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
