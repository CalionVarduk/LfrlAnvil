using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlView : SqlView
{
    internal PostgreSqlView(PostgreSqlSchema schema, PostgreSqlViewBuilder builder)
        : base( schema, builder, new PostgreSqlViewDataFieldCollection( builder.Source ) ) { }

    /// <inheritdoc cref="SqlView.DataFields" />
    public new PostgreSqlViewDataFieldCollection DataFields => ReinterpretCast.To<PostgreSqlViewDataFieldCollection>( base.DataFields );

    /// <inheritdoc cref="SqlView.Schema" />
    public new PostgreSqlSchema Schema => ReinterpretCast.To<PostgreSqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
