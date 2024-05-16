using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlViewDataField : SqlViewDataField
{
    internal MySqlViewDataField(MySqlView view, string name)
        : base( view, name ) { }

    /// <inheritdoc cref="SqlViewDataField.View" />
    public new MySqlView View => ReinterpretCast.To<MySqlView>( base.View );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
