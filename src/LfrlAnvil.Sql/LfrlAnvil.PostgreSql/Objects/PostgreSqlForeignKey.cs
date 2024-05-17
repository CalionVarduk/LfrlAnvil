using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlForeignKey : SqlForeignKey
{
    internal PostgreSqlForeignKey(PostgreSqlIndex originIndex, PostgreSqlIndex referencedIndex, PostgreSqlForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }

    /// <inheritdoc cref="SqlForeignKey.OriginIndex" />
    public new PostgreSqlIndex OriginIndex => ReinterpretCast.To<PostgreSqlIndex>( base.OriginIndex );

    /// <inheritdoc cref="SqlForeignKey.ReferencedIndex" />
    public new PostgreSqlIndex ReferencedIndex => ReinterpretCast.To<PostgreSqlIndex>( base.ReferencedIndex );

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
