using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlCheck : SqlCheck
{
    internal MySqlCheck(MySqlTable table, MySqlCheckBuilder builder)
        : base( table, builder ) { }

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
