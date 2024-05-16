using LfrlAnvil.MySql.Objects.Builders;
using LfrlAnvil.Sql.Objects;

namespace LfrlAnvil.MySql.Objects;

/// <inheritdoc />
/// <remarks><see cref="MySqlDialect"/> implementation.</remarks>
public sealed class MySqlIndex : SqlIndex
{
    internal MySqlIndex(MySqlTable table, MySqlIndexBuilder builder)
        : base( table, builder ) { }

    /// <inheritdoc cref="SqlIndex.Columns" />
    public new SqlIndexedArray<MySqlColumn> Columns => base.Columns.UnsafeReinterpretAs<MySqlColumn>();

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new MySqlTable Table => ReinterpretCast.To<MySqlTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new MySqlDatabase Database => ReinterpretCast.To<MySqlDatabase>( base.Database );
}
