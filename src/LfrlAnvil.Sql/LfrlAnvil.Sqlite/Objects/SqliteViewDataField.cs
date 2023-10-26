using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteViewDataField : SqliteObject, ISqlViewDataField
{
    private string? _fullName;
    private SqlViewDataFieldNode? _node;

    internal SqliteViewDataField(SqliteView view, string name)
        : base( name, SqlObjectType.ViewDataField )
    {
        View = view;
        _fullName = null;
        _node = null;
    }

    public SqliteView View { get; }
    public override SqliteDatabase Database => View.Database;
    public override string FullName => _fullName ??= SqliteHelpers.GetFullFieldName( View.FullName, Name );
    public SqlViewDataFieldNode Node => _node ??= View.RecordSet[Name];

    ISqlView ISqlViewDataField.View => View;
}
