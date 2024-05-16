using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteSchema : SqlSchema
{
    internal SqliteSchema(SqliteDatabase database, SqliteSchemaBuilder builder)
        : base( database, builder, new SqliteObjectCollection( builder.Objects ) ) { }

    /// <inheritdoc cref="SqlSchema.Objects" />
    public new SqliteObjectCollection Objects => ReinterpretCast.To<SqliteObjectCollection>( base.Objects );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );
}
