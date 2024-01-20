using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.MySql.Internal;

namespace LfrlAnvil.MySql.Objects;

public sealed class MySqlViewDataField : MySqlObject, ISqlViewDataField
{
    private string? _fullName;
    private SqlViewDataFieldNode? _node;

    internal MySqlViewDataField(MySqlView view, string name)
        : base( name, SqlObjectType.ViewDataField )
    {
        View = view;
        _fullName = null;
        _node = null;
    }

    public MySqlView View { get; }
    public override MySqlDatabase Database => View.Database;
    public override string FullName => _fullName ??= MySqlHelpers.GetFullFieldName( View.FullName, Name );
    public SqlViewDataFieldNode Node => _node ??= View.RecordSet[Name];

    ISqlView ISqlViewDataField.View => View;
}
