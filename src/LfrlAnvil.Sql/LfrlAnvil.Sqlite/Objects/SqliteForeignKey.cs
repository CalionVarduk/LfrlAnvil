using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteForeignKey : SqlForeignKey
{
    internal SqliteForeignKey(SqliteIndex originIndex, SqliteIndex referencedIndex, SqliteForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }

    public new SqliteIndex OriginIndex => ReinterpretCast.To<SqliteIndex>( base.OriginIndex );
    public new SqliteIndex ReferencedIndex => ReinterpretCast.To<SqliteIndex>( base.ReferencedIndex );
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
