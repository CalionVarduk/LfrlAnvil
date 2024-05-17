using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlSchema : SqlSchema
{
    internal PostgreSqlSchema(PostgreSqlDatabase database, PostgreSqlSchemaBuilder builder)
        : base( database, builder, new PostgreSqlObjectCollection( builder.Objects ) ) { }

    /// <inheritdoc cref="SqlSchema.Objects" />
    public new PostgreSqlObjectCollection Objects => ReinterpretCast.To<PostgreSqlObjectCollection>( base.Objects );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
