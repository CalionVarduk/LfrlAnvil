using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects;

public sealed class SqliteViewDataField : SqliteObject, ISqlViewDataField
{
    private SqlViewDataFieldNode? _node;

    internal SqliteViewDataField(SqliteView view, string name)
        : base( name, SqlObjectType.ViewDataField )
    {
        View = view;
        _node = null;
    }

    public SqliteView View { get; }
    public override SqliteDatabase Database => View.Database;
    public SqlViewDataFieldNode Node => _node ??= View.RecordSet[Name];

    ISqlView ISqlViewDataField.View => View;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
