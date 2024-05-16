using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteView : SqlView
{
    internal SqliteView(SqliteSchema schema, SqliteViewBuilder builder)
        : base( schema, builder, new SqliteViewDataFieldCollection( builder.Source ) ) { }

    /// <inheritdoc cref="SqlView.DataFields" />
    public new SqliteViewDataFieldCollection DataFields => ReinterpretCast.To<SqliteViewDataFieldCollection>( base.DataFields );

    /// <inheritdoc cref="SqlView.Schema" />
    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteView"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }
}
