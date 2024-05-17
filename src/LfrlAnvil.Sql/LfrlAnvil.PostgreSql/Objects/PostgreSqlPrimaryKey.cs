using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlPrimaryKey : SqlPrimaryKey
{
    internal PostgreSqlPrimaryKey(PostgreSqlIndex index, PostgreSqlPrimaryKeyBuilder builder)
        : base( index, builder ) { }

    /// <inheritdoc cref="SqlPrimaryKey.Index" />
    public new PostgreSqlIndex Index => ReinterpretCast.To<PostgreSqlIndex>( base.Index );

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
