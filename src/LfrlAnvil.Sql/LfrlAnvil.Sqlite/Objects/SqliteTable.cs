using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteTable : SqlTable
{
    internal SqliteTable(SqliteSchema schema, SqliteTableBuilder builder)
        : base( schema, builder, new SqliteColumnCollection( builder.Columns ), new SqliteConstraintCollection( builder.Constraints ) ) { }

    /// <inheritdoc cref="SqlTable.Columns" />
    public new SqliteColumnCollection Columns => ReinterpretCast.To<SqliteColumnCollection>( base.Columns );

    /// <inheritdoc cref="SqlTable.Constraints" />
    public new SqliteConstraintCollection Constraints => ReinterpretCast.To<SqliteConstraintCollection>( base.Constraints );

    /// <inheritdoc cref="SqlTable.Schema" />
    public new SqliteSchema Schema => ReinterpretCast.To<SqliteSchema>( base.Schema );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteTable"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }
}
