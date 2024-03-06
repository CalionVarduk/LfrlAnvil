using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteTable : SqlTable
{
    internal SqliteTable(SqliteSchema schema, SqliteTableBuilder builder)
        : base( schema, builder, new SqliteColumnCollection( builder.Columns ), new SqliteConstraintCollection( builder.Constraints ) ) { }

    public new SqliteColumnCollection Columns => ReinterpretCast.To<SqliteColumnCollection>( base.Columns );
    public new SqliteConstraintCollection Constraints => ReinterpretCast.To<SqliteConstraintCollection>( base.Constraints );
    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }
}
