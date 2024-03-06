using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteIndex : SqlIndex
{
    internal SqliteIndex(SqliteTable table, SqliteIndexBuilder builder)
        : base( table, builder ) { }

    public new SqlIndexColumnArray<SqliteColumn> Columns => base.Columns.UnsafeReinterpretAs<SqliteColumn>();
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
