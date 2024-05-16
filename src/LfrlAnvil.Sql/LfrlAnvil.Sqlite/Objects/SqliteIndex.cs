using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteIndex : SqlIndex
{
    internal SqliteIndex(SqliteTable table, SqliteIndexBuilder builder)
        : base( table, builder ) { }

    /// <inheritdoc cref="SqlIndex.Columns" />
    public new SqlIndexedArray<SqliteColumn> Columns => base.Columns.UnsafeReinterpretAs<SqliteColumn>();

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteIndex"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
