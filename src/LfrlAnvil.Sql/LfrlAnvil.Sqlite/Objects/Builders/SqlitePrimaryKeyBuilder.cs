using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqlitePrimaryKeyBuilder : SqlPrimaryKeyBuilder
{
    internal SqlitePrimaryKeyBuilder(SqliteIndexBuilder index, string name)
        : base( index, name ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlConstraintBuilder.Table" />
    public new SqliteTableBuilder Table => ReinterpretCast.To<SqliteTableBuilder>( base.Table );

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.Index" />
    public new SqliteIndexBuilder Index => ReinterpretCast.To<SqliteIndexBuilder>( base.Index );

    /// <summary>
    /// Returns a string representation of this <see cref="SqlitePrimaryKeyBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Table.Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetName(string)" />
    public new SqlitePrimaryKeyBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }

    /// <inheritdoc cref="SqlPrimaryKeyBuilder.SetDefaultName()" />
    public new SqlitePrimaryKeyBuilder SetDefaultName()
    {
        base.SetDefaultName();
        return this;
    }
}
