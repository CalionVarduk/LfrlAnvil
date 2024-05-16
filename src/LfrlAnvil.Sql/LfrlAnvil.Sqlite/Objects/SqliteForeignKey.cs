using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects;
using LfrlAnvil.Sqlite.Internal;
using LfrlAnvil.Sqlite.Objects.Builders;

namespace LfrlAnvil.Sqlite.Objects;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteForeignKey : SqlForeignKey
{
    internal SqliteForeignKey(SqliteIndex originIndex, SqliteIndex referencedIndex, SqliteForeignKeyBuilder builder)
        : base( originIndex, referencedIndex, builder ) { }

    /// <inheritdoc cref="SqlForeignKey.OriginIndex" />
    public new SqliteIndex OriginIndex => ReinterpretCast.To<SqliteIndex>( base.OriginIndex );

    /// <inheritdoc cref="SqlForeignKey.ReferencedIndex" />
    public new SqliteIndex ReferencedIndex => ReinterpretCast.To<SqliteIndex>( base.ReferencedIndex );

    /// <inheritdoc cref="SqlConstraint.Table" />
    public new SqliteTable Table => ReinterpretCast.To<SqliteTable>( base.Table );

    /// <inheritdoc cref="SqlObject.Database" />
    public new SqliteDatabase Database => ReinterpretCast.To<SqliteDatabase>( base.Database );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteForeignKey"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }
}
