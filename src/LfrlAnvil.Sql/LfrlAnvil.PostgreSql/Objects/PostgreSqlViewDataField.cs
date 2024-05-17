using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlViewDataField : SqlViewDataField
{
    internal PostgreSqlViewDataField(PostgreSqlView view, string name)
        : base( view, name ) { }

    /// <inheritdoc cref="SqlViewDataField.View" />
    public new PostgreSqlView View => ReinterpretCast.To<PostgreSqlView>( base.View );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
