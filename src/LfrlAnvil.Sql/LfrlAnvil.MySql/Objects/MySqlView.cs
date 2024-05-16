using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlView : SqlView
{
    internal MySqlView(MySqlSchema schema, MySqlViewBuilder builder)
        : base( schema, builder, new MySqlViewDataFieldCollection( builder.Source ) ) { }

    /// <inheritdoc cref="SqlView.DataFields" />
    public new MySqlViewDataFieldCollection DataFields => ReinterpretCast.To<MySqlViewDataFieldCollection>( base.DataFields );

    /// <inheritdoc cref="SqlView.Schema" />
    public new MySqlSchema Schema => ReinterpretCast.To<MySqlSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
