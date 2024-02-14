using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects;

public abstract class SqlViewDataField : SqlObject, ISqlViewDataField
{
    private SqlViewDataFieldNode? _node;

    protected SqlViewDataField(SqlView view, string name)
        : base( view.Database, SqlObjectType.ViewDataField, name )
    {
        View = view;
        _node = null;
    }

    public SqlView View { get; }
    public SqlViewDataFieldNode Node => _node ??= View.Node[Name];

    ISqlView ISqlViewDataField.View => View;

    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
