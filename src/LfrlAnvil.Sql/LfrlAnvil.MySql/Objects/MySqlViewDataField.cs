using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Internal;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlViewDataField : MySqlObject, ISqlViewDataField
{
    private SqlViewDataFieldNode? _node;

    internal MySqlViewDataField(MySqlView view, string name)
        : base( name, SqlObjectType.ViewDataField )
    {
        View = view;
        _node = null;
    }

    public MySqlView View { get; }
    public override MySqlDatabase Database => View.Database;
    public SqlViewDataFieldNode Node => _node ??= View.Node[Name];

    ISqlView ISqlViewDataField.View => View;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {MySqlHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
