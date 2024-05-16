using System.Diagnostics.Contracts;
using LfrlAnvil.Sql;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteForeignKeyBuilder : SqlForeignKeyBuilder
{
    internal SqliteForeignKeyBuilder(SqliteIndexBuilder originIndex, SqliteIndexBuilder referencedIndex, string name)
        : base( originIndex, referencedIndex, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteForeignKeyBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetName(string)" />
    public new SqliteForeignKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetDefaultName()" />
    public new SqliteForeignKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnDeleteBehavior(ReferenceBehavior)" />
    public new SqliteForeignKeyBuilder SetOnDeleteBehavior(ReferenceBehavior behavior)
    {
        base.SetOnDeleteBehavior( behavior );
        return this;
    }

    /// <inheritdoc cref="SqlForeignKeyBuilder.SetOnUpdateBehavior(ReferenceBehavior)" />
    public new SqliteForeignKeyBuilder SetOnUpdateBehavior(ReferenceBehavior behavior)
    {
        base.SetOnUpdateBehavior( behavior );
        return this;
    }
}
