using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlForeignKey : SqlForeignKey
{
    internal MySqlForeignKey(MySqlIndex originIndex, MySqlIndex referencedIndex, MySqlForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }

    /// <inheritdoc cref="SqlForeignKey.OriginIndex" />
    public new MySqlIndex OriginIndex => ReinterpretCast.To<MySqlIndex>( base.OriginIndex );

    /// <inheritdoc cref="SqlForeignKey.ReferencedIndex" />
    public new MySqlIndex ReferencedIndex => ReinterpretCast.To<MySqlIndex>( base.ReferencedIndex );

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
