using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlPrimaryKey : SqlPrimaryKey
{
    internal MySqlPrimaryKey(MySqlIndex index, MySqlPrimaryKeyBuilder builder)
        : base( index, builder ) { }

    /// <inheritdoc cref="SqlPrimaryKey.Index" />
    public new MySqlIndex Index => ReinterpretCast.To<MySqlIndex>( base.Index );

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
