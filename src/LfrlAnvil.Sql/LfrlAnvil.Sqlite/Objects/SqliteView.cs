using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteView : SqlView
{
    internal SqliteView(SqliteSchema schema, SqliteViewBuilder builder)
        : base( schema, builder, new SqliteViewDataFieldCollection( builder.Source ) ) { }

    public new SqliteViewDataFieldCollection DataFields => ReinterpretCast.To<SqliteViewDataFieldCollection>( base.DataFields );
    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }
}
