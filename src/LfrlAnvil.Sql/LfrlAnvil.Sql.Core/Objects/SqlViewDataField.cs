using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions.Objects;
using LfrlAnvil.Sql.Internal;

namespace LfrlAnvil.Sql.Objects;

/// <inheritdoc cref="ISqlViewDataField" />
public abstract class SqlViewDataField : SqlObject, ISqlViewDataField
{
    private SqlViewDataFieldNode? _node;

    /// <summary>
    /// Creates a new <see cref="SqlViewDataField"/> instance.
    /// </summary>
    /// <param name="view">View that this data field belongs to.</param>
    /// <param name="name">Data field's name.</param>
    protected SqlViewDataField(SqlView view, string name)
        : base( view.Database, SqlObjectType.ViewDataField, name )
    {
        View = view;
        _node = null;
    }

    /// <inheritdoc cref="ISqlViewDataField.View" />
    public SqlView View { get; }

    /// <inheritdoc />
    public SqlViewDataFieldNode Node => _node ??= View.Node[Name];

    ISqlView ISqlViewDataField.View => View;

    /// <summary>
    /// Returns a string representation of this <see cref="SqlViewDataField"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqlHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
