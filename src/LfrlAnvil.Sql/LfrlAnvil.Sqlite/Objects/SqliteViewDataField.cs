using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteViewDataField : SqlViewDataField
{
    internal SqliteViewDataField(SqliteView view, string name)
        : base( view, name ) { }

    /// <inheritdoc cref="SqlViewDataField.View" />
    public new SqliteView View => ReinterpretCast.To<SqliteView>( base.View );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteViewDataField"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( View.Schema.Name, View.Name, Name )}";
    }
}
