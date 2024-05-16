using System.Diagnostics.Contracts;
using LfrlAnvil.Sql.Expressions;
using LfrlAnvil.Sql.Objects.Builders;
using LfrlAnvil.Sqlite.Internal;

namespace LfrlAnvil.Sqlite.Objects.Builders;

/// <inheritdoc />
/// <remarks><see cref="SqliteDialect"/> implementation.</remarks>
public sealed class SqliteViewBuilder : SqlViewBuilder
{
    internal SqliteViewBuilder(
        SqliteSchemaBuilder schema,
        string name,
        SqlQueryExpressionNode source,
        ReadOnlyArray<SqlObjectBuilder> referencedObjects)
        : base( schema, name, source, referencedObjects ) { }

    /// <inheritdoc cref="SqlObjectBuilder.Database" />
    public new SqliteDatabaseBuilder Database => ReinterpretCast.To<SqliteDatabaseBuilder>( base.Database );

    /// <inheritdoc cref="SqlViewBuilder.Schema" />
    public new SqliteSchemaBuilder Schema => ReinterpretCast.To<SqliteSchemaBuilder>( base.Schema );

    /// <summary>
    /// Returns a string representation of this <see cref="SqliteViewBuilder"/> instance.
    /// </summary>
    /// <returns>String representation.</returns>
    [Pure]
    public override string ToString()
    {
        return $"[{Type}] {SqliteHelpers.GetFullName( Schema.Name, Name )}";
    }

    /// <inheritdoc cref="SqlViewBuilder.SetName(string)" />
    public new SqliteViewBuilder SetName(string name)
    {
        base.SetName( name );
        return this;
    }
}
