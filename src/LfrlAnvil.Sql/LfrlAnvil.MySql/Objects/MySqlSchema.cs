using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlSchema : SqlSchema
{
    internal MySqlSchema(MySqlDatabase database, MySqlSchemaBuilder builder)
        : base( database, builder, new MySqlObjectCollection( builder.Objects ) ) { }

    /// <inheritdoc cref="SqlSchema.Objects" />
    public new MySqlObjectCollection Objects => ReinterpretCast.To<MySqlObjectCollection>( base.Objects );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
