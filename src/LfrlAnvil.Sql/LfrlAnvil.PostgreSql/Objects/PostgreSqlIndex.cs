using LfrlAnvil.PostgreSql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.PostgreSql.Objects;

/// <inheritdoc />
/// <remarks><see cref="PostgreSqlDialect"/> implementation.</remarks>
public sealed class PostgreSqlIndex : SqlIndex
{
    internal PostgreSqlIndex(PostgreSqlTable table, PostgreSqlIndexBuilder builder)
        : base( table, builder ) { }

    /// <inheritdoc cref="SqlIndex.Columns" />
    public new SqlIndexedArray<PostgreSqlColumn> Columns => base.Columns.UnsafeReinterpretAs<PostgreSqlColumn>();

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new PostgreSqlTable Table => ReinterpretCast.To<PostgreSqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new PostgreSqlDatabase Database => ReinterpretCast.To<PostgreSqlDatabase>( base.Database );
}
