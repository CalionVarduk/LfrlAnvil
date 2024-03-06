using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteViewDataField : SqlViewDataField
{
    internal SqliteViewDataField(SqliteView view, string name)
        : base( view, name ) { }

    public new SqliteView View => ReinterpretCast.To<SqliteView>( base.View );
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
