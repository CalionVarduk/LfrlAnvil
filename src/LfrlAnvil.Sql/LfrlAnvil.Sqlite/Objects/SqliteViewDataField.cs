using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteViewDataField : SqliteObject, ISqlViewDataField
{
    private string? _fullName;

    internal SqliteViewDataField(SqliteView view, string name)
        : base( name, SqlObjectType.ViewDataField )
    {
        View = view;
        _fullName = null;
    }

    public SqliteView View { get; }
    public override SqliteDatabase Database => View.Database;
    public override string FullName => _fullName ??= SqliteHelpers.GetFullFieldName( View.FullName, Name );

    ISqlView ISqlViewDataField.View => View;
}
