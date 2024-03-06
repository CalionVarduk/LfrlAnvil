using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteSchema : SqlSchema
{
    internal SqliteSchema(SqliteDatabase database, SqliteSchemaBuilder builder)
        : base( database, builder, new SqliteObjectCollection( builder.Objects ) ) { }

    public new SqliteObjectCollection Objects => ReinterpretCast.To<SqliteObjectCollection>( base.Objects );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );
}
