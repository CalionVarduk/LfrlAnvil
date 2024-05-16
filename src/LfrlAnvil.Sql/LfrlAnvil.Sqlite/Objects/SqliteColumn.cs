using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteColumn : SqlColumn
{
    internal SqliteColumn(SqliteTable table, SqliteColumnBuilder builder)
        : base( table, builder ) { }

    /// <inheritdoc cref="SqlColumn.Table" />
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteColumn"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Table.Name, Name )}";
    }
}
